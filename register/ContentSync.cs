using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace register
{
    class ContentSync
    {
        class ConfigCS
        {
            public string ApiKey;
            public string FirebaseEmail;
            public string FirebasePassword;
        }
        string ApiKey;
        string FirebaseEmail;
        string FirebasePassword;
        public ContentSync()
        {
            //load config
            string path = ConfigMng.findTmpl("fb.cfg");
            string txt = File.ReadAllText(path);
            ConfigCS cfg = JsonConvert.DeserializeObject<ConfigCS>(txt);
            ApiKey = cfg.ApiKey;
            FirebaseEmail = cfg.FirebaseEmail;
            FirebasePassword = cfg.FirebasePassword;
        }


        public class MyTableInfo
        {
            public string ids;
        }
        public async Task SyncTableAsync(string table)
        {
            //get table info on firebase 
            List<UInt64> ids = await GetIdsAsync(table);
            //open local table and sync data
            
        }

        public async Task<object> GetObjAsync(string table, UInt64 id)
        {
            object obj = null;
            var authProvider = new FirebaseAuthProvider(new FirebaseConfig(ApiKey));
            var auth = authProvider.SignInWithEmailAndPasswordAsync(FirebaseEmail, FirebasePassword).Result;
            var access_token = auth.FirebaseToken;
            string qry = "https://testfirebase-18749.firebaseio.com/<table>/<id>.json?auth=<ACCESS_TOKEN>";
            qry = qry.Replace("<DATABASE_NAME>","testfirebase-18749");
            qry = qry.Replace("<ACCESS_TOKEN>", access_token);
            qry = qry.Replace("<table>", table);
            qry = qry.Replace("<id>", id.ToString());

            var client = new HttpClient();
            HttpResponseMessage response = await client.GetAsync(qry);
            // Get the response content.
            HttpContent responseContent = response.Content;

            // Get the stream of the content.
            using (var reader = new StreamReader(await responseContent.ReadAsStreamAsync()))
            {
                var text = await reader.ReadToEndAsync();
                obj = JsonConvert.DeserializeObject(text);
            }
            return obj;
        }
        public async Task<List<object>> GetObjAsync(string table, List<UInt64> ids)
        {
            List<object> objs = new List<object>();
            var authProvider = new FirebaseAuthProvider(new FirebaseConfig(ApiKey));
            var auth = authProvider.SignInWithEmailAndPasswordAsync(FirebaseEmail, FirebasePassword).Result;
            var access_token = auth.FirebaseToken;
            string qry = "https://testfirebase-18749.firebaseio.com/<table>/<id>.json?auth=<ACCESS_TOKEN>";
            qry = qry.Replace("<DATABASE_NAME>","testfirebase-18749");
            qry = qry.Replace("<ACCESS_TOKEN>", access_token);
            qry = qry.Replace("<table>", table);

            var client = new HttpClient();
            
            foreach (UInt64 id in ids)
            {
                HttpResponseMessage response = await client.GetAsync(qry.Replace("<id>", id.ToString()));
                // Get the response content.
                HttpContent responseContent = response.Content;

                // Get the stream of the content.
                using (var reader = new StreamReader(await responseContent.ReadAsStreamAsync()))
                {
                    var text = await reader.ReadToEndAsync();
                    var obj = JsonConvert.DeserializeObject<MyLog>(text);
                    objs.Add(obj);
                }
            }
            return objs;
        }

        public async Task<List<ulong>> GetIdsAsync(string table)
        {
            List<UInt64> ids = new List<ulong>();
            var authProvider = new FirebaseAuthProvider(new FirebaseConfig(ApiKey));
            var auth = authProvider.SignInWithEmailAndPasswordAsync(FirebaseEmail, FirebasePassword).Result;
            var access_token = auth.FirebaseToken;
            string qry = "https://testfirebase-18749.firebaseio.com/<table>/info.json?auth=<ACCESS_TOKEN>";
            qry = qry.Replace("<DATABASE_NAME>","testfirebase-18749");
            qry = qry.Replace("<ACCESS_TOKEN>", access_token);
            qry = qry.Replace("<table>", table);

            var client = new HttpClient();
            HttpResponseMessage response = await client.GetAsync(qry);
            // Get the response content.
            HttpContent responseContent = response.Content;

            // Get the stream of the content.
            using (var reader = new StreamReader(await responseContent.ReadAsStreamAsync()))
            {
                // Write the output.
                var text = await reader.ReadToEndAsync();
                var info = JsonConvert.DeserializeObject<MyTableInfo>(text) ;
                var arr = info.ids.Split(new char[] { ',' },StringSplitOptions.RemoveEmptyEntries);
                foreach (var zid in arr)
                {
                    ids.Add(UInt64.Parse(zid));
                }
            }

            return ids;
        }
        public class FirebaseConfig
        {
            public string ApiKey;
            public FirebaseConfig(string apiKey)
            {
                this.ApiKey = apiKey;
            }
        }

        public class FirebaseAuth
        {
        }
        public class FirebaseAuthLink : FirebaseAuth
        {
            [JsonProperty("idToken")]
            public string FirebaseToken;
        }

        public async Task PutLogAsync(string table, List<MyLog> checkInLst)
        {
            var authProvider = new FirebaseAuthProvider(new FirebaseConfig(ApiKey));
            var auth = authProvider.SignInWithEmailAndPasswordAsync(FirebaseEmail, FirebasePassword).Result;
            var access_token = auth.FirebaseToken;
            string qry = "https://testfirebase-18749.firebaseio.com/<table>/<id>.json?auth=<ACCESS_TOKEN>";
            qry = qry.Replace("<DATABASE_NAME>","testfirebase-18749");
            qry = qry.Replace("<ACCESS_TOKEN>", access_token);
            qry = qry.Replace("<table>", table);

            var client = new HttpClient();
            
            foreach (MyLog log in checkInLst)
            {
                string payload = JsonConvert.SerializeObject(log);
                var c = new StringContent(payload, Encoding.UTF8, "application/json" );
                var url = qry.Replace("<id>", log.id.ToString());
                HttpResponseMessage response = await client.PutAsync(url, c);
                
                // Get the response content.
                HttpContent responseContent = response.Content;

                // Get the stream of the content.
                using (var reader = new StreamReader(await responseContent.ReadAsStreamAsync()))
                {
                    var text = await reader.ReadToEndAsync();
                }
            }
        }

        public class FirebaseAuthProvider
        {
            private const string GooglePasswordUrl = "https://www.googleapis.com/identitytoolkit/v3/relyingparty/verifyPassword?key={0}";


            private readonly FirebaseConfig authConfig;
            private readonly HttpClient client;
            public FirebaseAuthProvider(FirebaseConfig authConfig)
            {
                this.authConfig = authConfig;
                this.client = new HttpClient();
            }
            public async Task<FirebaseAuthLink> SignInWithEmailAndPasswordAsync(string email, string password,
            string tenantId = null)
            {
                StringBuilder sb = new StringBuilder($"{{\"email\":\"{email}\",\"password\":\"{password}\",");

                if (tenantId != null)
                {
                    sb.Append($"\"tenantId\":\"{tenantId}\",");
                }

                sb.Append("\"returnSecureToken\":true}");

                FirebaseAuthLink firebaseAuthLink = await this.ExecuteWithPostContentAsync(GooglePasswordUrl, sb.ToString()).ConfigureAwait(false);
                return firebaseAuthLink;
            }
            private async Task<FirebaseAuthLink> ExecuteWithPostContentAsync(string googleUrl, string postContent)
            {
                string responseData = "N/A";

                try
                {
                    var response = await this.client.PostAsync(new Uri(string.Format(googleUrl, this.authConfig.ApiKey)), new StringContent(postContent, Encoding.UTF8, "application/json")).ConfigureAwait(false);
                    responseData = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

                    response.EnsureSuccessStatusCode();

                    //var user = JsonConvert.DeserializeObject<User>(responseData);
                    var auth = JsonConvert.DeserializeObject<FirebaseAuthLink>(responseData);

                    return auth;
                }
                catch (Exception ex)
                {
                    AuthErrorReason errorReason = GetFailureReason(responseData);
                    throw new FirebaseAuthException(googleUrl, postContent, responseData, ex, errorReason);
                }
            }
            /// <summary>
            /// Resolves failure reason flags based on the returned error code.
            /// </summary>
            /// <remarks>Currently only provides support for failed email auth flags.</remarks>
            private static AuthErrorReason GetFailureReason(string responseData)
            {
                var failureReason = AuthErrorReason.Undefined;
                try
                {
                    if (!string.IsNullOrEmpty(responseData) && responseData != "N/A")
                    {
                        //create error data template and try to parse JSON
                        var errorData = new { error = new { code = 0, message = "errorid" } };
                        errorData = JsonConvert.DeserializeAnonymousType(responseData, errorData);

                        //errorData is just null if different JSON was received
                        switch (errorData?.error?.message)
                        {
                            //general errors
                            case "invalid access_token, error code 43.":
                                failureReason = AuthErrorReason.InvalidAccessToken;
                                break;

                            case "CREDENTIAL_TOO_OLD_LOGIN_AGAIN":
                                failureReason = AuthErrorReason.LoginCredentialsTooOld;
                                break;

                            case "OPERATION_NOT_ALLOWED":
                                failureReason = AuthErrorReason.OperationNotAllowed;
                                break;

                            //possible errors from Third Party Authentication using GoogleIdentityUrl
                            case "INVALID_PROVIDER_ID : Provider Id is not supported.":
                                failureReason = AuthErrorReason.InvalidProviderID;
                                break;
                            case "MISSING_REQUEST_URI":
                                failureReason = AuthErrorReason.MissingRequestURI;
                                break;
                            case "A system error has occurred - missing or invalid postBody":
                                failureReason = AuthErrorReason.SystemError;
                                break;
                            case "MISSING_OR_INVALID_NONCE : Duplicate credential received. Please try again with a new credential.":
                                failureReason = AuthErrorReason.DuplicateCredentialUse;
                                break;

                            //possible errors from Email/Password Account Signup (via signupNewUser or setAccountInfo) or Signin
                            case "INVALID_EMAIL":
                                failureReason = AuthErrorReason.InvalidEmailAddress;
                                break;
                            case "MISSING_PASSWORD":
                                failureReason = AuthErrorReason.MissingPassword;
                                break;

                            //possible errors from Email/Password Account Signup (via signupNewUser or setAccountInfo)
                            case "EMAIL_EXISTS":
                                failureReason = AuthErrorReason.EmailExists;
                                break;

                            //possible errors from Account Delete
                            case "USER_NOT_FOUND":
                                failureReason = AuthErrorReason.UserNotFound;
                                break;

                            //possible errors from Email/Password Signin
                            case "INVALID_PASSWORD":
                                failureReason = AuthErrorReason.WrongPassword;
                                break;
                            case "EMAIL_NOT_FOUND":
                                failureReason = AuthErrorReason.UnknownEmailAddress;
                                break;
                            case "USER_DISABLED":
                                failureReason = AuthErrorReason.UserDisabled;
                                break;

                            //possible errors from Email/Password Signin or Password Recovery or Email/Password Sign up using setAccountInfo
                            case "MISSING_EMAIL":
                                failureReason = AuthErrorReason.MissingEmail;
                                break;
                            case "RESET_PASSWORD_EXCEED_LIMIT":
                                failureReason = AuthErrorReason.ResetPasswordExceedLimit;
                                break;

                            //possible errors from Password Recovery
                            case "MISSING_REQ_TYPE":
                                failureReason = AuthErrorReason.MissingRequestType;
                                break;

                            //possible errors from Account Linking
                            case "INVALID_ID_TOKEN":
                                failureReason = AuthErrorReason.InvalidIDToken;
                                break;

                            //possible errors from Getting Linked Accounts
                            case "INVALID_IDENTIFIER":
                                failureReason = AuthErrorReason.InvalidIdentifier;
                                break;
                            case "MISSING_IDENTIFIER":
                                failureReason = AuthErrorReason.MissingIdentifier;
                                break;
                            case "FEDERATED_USER_ID_ALREADY_LINKED":
                                failureReason = AuthErrorReason.AlreadyLinked;
                                break;
                        }

                        if (failureReason == AuthErrorReason.Undefined)
                        {
                            //possible errors from Email/Password Account Signup (via signupNewUser or setAccountInfo)
                            if (errorData?.error?.message?.StartsWith("WEAK_PASSWORD :") ?? false) failureReason = AuthErrorReason.WeakPassword;
                            //possible errors from Email/Password Signin
                            else if (errorData?.error?.message?.StartsWith("TOO_MANY_ATTEMPTS_TRY_LATER :") ?? false) failureReason = AuthErrorReason.TooManyAttemptsTryLater;
                            //ID Token issued is stale to sign-in (e.g. with Apple)
                            else if (errorData?.error?.message?.StartsWith("ERROR_INVALID_CREDENTIAL") ?? false) failureReason = AuthErrorReason.StaleIDToken;
                        }
                    }
                }
                catch (JsonReaderException)
                {
                    //the response wasn't JSON - no data to be parsed
                }
                catch (Exception e)
                {
                    Debug.WriteLine($"Unexpected error trying to parse the response: {e}");
                }

                return failureReason;
            }
        }

        public class FirebaseAuthException : Exception
        {
            public FirebaseAuthException(string requestUrl, string requestData, string responseData, Exception innerException, AuthErrorReason reason = AuthErrorReason.Undefined)
                : base(GenerateExceptionMessage(requestUrl, requestData, responseData, reason), innerException)
            {
                this.RequestUrl = requestUrl;
                this.RequestData = requestData;
                this.ResponseData = responseData;
                this.Reason = reason;
            }

            /// <summary>
            /// Post data passed to the authentication service.
            /// </summary>
            public string RequestData
            {
                get;
            }

            public string RequestUrl
            {
                get;
            }

            /// <summary>
            /// Response from the authentication service.
            /// </summary>
            public string ResponseData
            {
                get;
            }

            /// <summary>
            /// indicates why a login failed. If not resolved, defaults to
            /// <see cref="AuthErrorReason.Undefined"/>.
            /// </summary>
            public AuthErrorReason Reason
            {
                get;
            }

            private static string GenerateExceptionMessage(string requestUrl, string requestData, string responseData, AuthErrorReason errorReason)
            {
                return $"Exception occured while authenticating.\nUrl: {requestUrl}\nRequest Data: {requestData}\nResponse: {responseData}\nReason: {errorReason}";
            }
        }
        void init()
        {
        }

        public enum AuthErrorReason
        {
            /// <summary>
            /// Unknown error reason.
            /// </summary>
            Undefined,
            /// <summary>
            /// The sign in method is not enabled.
            /// </summary>
            OperationNotAllowed,
            /// <summary>
            /// The user was disabled and is not granted access anymore.
            /// </summary>
            UserDisabled,
            /// <summary>
            /// The user was not found
            /// </summary>
            UserNotFound,
            /// <summary>
            /// Third-party Auth Providers: PostBody does not contain or contains invalid Authentication Provider string.
            /// </summary>
            InvalidProviderID,
            /// <summary>
            /// Third-party Auth Providers: PostBody does not contain or contains invalid Access Token string obtained from Auth Provider.
            /// </summary>
            InvalidAccessToken,
            /// <summary>
            /// Changes to user's account has been made since last log in. User needs to login again.
            /// </summary>
            LoginCredentialsTooOld,
            /// <summary>
            /// Third-party Auth Providers: Request does not contain a value for parameter: requestUri.
            /// </summary>
            MissingRequestURI,
            /// <summary>
            /// Third-party Auth Providers: Request does not contain a value for parameter: postBody.
            /// </summary>
            SystemError,
            /// <summary>
            /// Email/Password Authentication: Email address is not in valid format.
            /// </summary>
            InvalidEmailAddress,
            /// <summary>
            /// Email/Password Authentication: No password provided!
            /// </summary>
            MissingPassword,
            /// <summary>
            /// Email/Password Signup: Password must be more than 6 characters.  This error could also be caused by attempting to create a user account using Set Account Info without supplying a password for the new user.
            /// </summary>
            WeakPassword,
            /// <summary>
            /// Email/Password Signup: Email address already connected to another account. This error could also be caused by attempting to create a user account using Set Account Info and an email address already linked to another account.
            /// </summary>
            EmailExists,
            /// <summary>
            /// Email/Password Signin: No email provided! This error could also be caused by attempting to create a user account using Set Account Info without supplying an email for the new user.
            /// </summary>
            MissingEmail,
            /// <summary>
            /// Email/Password Signin: No user with a matching email address is registered.
            /// </summary>
            UnknownEmailAddress,
            /// <summary>
            /// Email/Password Signin: The supplied password is not valid for the email address.
            /// </summary>
            WrongPassword,
            /// <summary>
            /// Email/Password Signin: Too many password login have been attempted. Try again later.
            /// </summary>
            TooManyAttemptsTryLater,
            /// <summary>
            /// Password Recovery: Request does not contain a value for parameter: requestType or supplied value is invalid.
            /// </summary>
            MissingRequestType,
            /// <summary>
            /// Password Recovery: Reset password limit exceeded.
            /// </summary>
            ResetPasswordExceedLimit,
            /// <summary>
            /// Account Linking: Authenticated User ID Token is invalid!
            /// </summary>
            InvalidIDToken,
            /// <summary>
            /// Linked Accounts: Request does not contain a value for parameter: identifier.
            /// </summary>
            MissingIdentifier,
            /// <summary>
            /// Linked Accounts: Request contains an invalid value for parameter: identifier.
            /// </summary>
            InvalidIdentifier,
            /// <summary>
            /// Linked accounts: account to link has already been linked.
            /// </summary>
            AlreadyLinked,
            /// <summary>
            /// Third-party Auth Providers: PostBody contains an Id Token string obtained from Auth Provider which is actually stale.
            /// </summary>
            StaleIDToken,
            /// <summary>
            /// Third-party Auth Providers: PostBody contains an Id Token string obtained from Auth Provider which has already been received.
            /// </summary>
            DuplicateCredentialUse
        }
    }
}
