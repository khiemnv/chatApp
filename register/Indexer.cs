using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace register
{

    class Indexer
    {
        
    public class MyHeap<T>
    {
        T[] h;
        Func<T, T, int> comp;
        int len;
        public MyHeap(T[] arr, Func<T, T, int> comp)
        {
            h = arr;
            this.comp = comp;
            len = arr.Length;
        }

        void BuildMinHeap()
        {
            for (int i = len / 2; i >= 0; i--)
            {
                MinHeap(i);
            }
        }
        void MinHeap(int i)
        {
            int m = (i + 1) * 2;
            if (m < len)
            {
                if (comp(h[m - 1], h[m]) < 0)
                {
                    m--;
                }
            }
            else if (m == len)
            {
                m--;
            }
            else
            {
                //do nothing
                return;
            }

            if (comp(h[m], h[i]) < 0)
            {
                swap(i, m);
                MinHeap(m);
            }
        }
        void swap(int i, int m)
        {
            T tmp = h[m];
            h[m] = h[i];
            h[i] = tmp;
        }
        public T PopMin()
        {
            BuildMinHeap();
            T tmp = h[0];
            len--;
            swap(0, len);
            return tmp;
        }
    }
            public class SrchResult
    {
        public SrchRec[] recs;
        public List<UInt64> titles;
        public List<UInt64> paragraphs;
        public string[] items;
    }
    public class SrchRec
    {
        public int[] path;
        public int d;
        public List<MyWord> detail;
    }
                    public class MyWord
    {
        public string content;
        public UInt64 titleId;
        public UInt64 parId;
        public int pos;
        public string key;
    }
        //buid data
        public void init(List<string> items)
        {
            var reg = new Regex(@"[\w]+(-\w+)*");
            reg = new Regex(@"[\w]+");
            var dict = new HashSet<string>();
            var lst = new List<MyWord>();
            var maxKeyLen = 0;
            myKey keygen = new myKey();
            int idx = 0;
            m_items = new List<string>();
            foreach (string txt in items)
            {
                var mc = reg.Matches(txt);
                foreach (Match m in mc)
                {
                    bool chk;
                    var key = keygen.genKey2(m.Value, out chk);
                    if (!chk)
                    {
                        Debug.WriteLine("{0} {1}", m.Index, m.Value);
                    }
                    if (key.Length > maxKeyLen) { maxKeyLen = key.Length; }
                    lst.Add(new MyWord()
                    {
                        content = m.Value,
                        parId = (UInt64)idx,
                        key = key,
                        pos = m.Index
                    });
                    dict.Add(key);
                }
                idx++;
                m_items.Add(txt);
            }
            AddToSearchDb(dict,lst);
        }

        Dictionary<string,List<MyWord>> m_db;
        List<string> m_items;
        void AddToSearchDb(HashSet<string> keysH, List<MyWord> wordsL)
        {
            m_db = new Dictionary<string, List<MyWord>>();
            foreach(MyWord wd in wordsL)
            {
                List<MyWord> tlst;
                if (m_db.ContainsKey(wd.key))
                {
                    tlst = m_db[wd.key];
                } else
                {
                    tlst  = new List<MyWord>();
                    m_db.Add(wd.key,tlst);
                }
                tlst.Add(wd);
            }
        }

        static Dictionary<string, string> dict = new Dictionary<string, string>() {
                {"ĐÐđ", "d"     },
                {"áàảãạÁÀẢÃẠ", "a" },
                {"éèẻẽẹÉÈẺẼẸ", "e" },
                {"íìĩỉịÍÌĨỈỊ", "i" },
                {"òóỏõọÒÓỎÕỌ", "o" },
                {"úùủũụÚÙỦŨỤ", "u" },
                {"ỳýỹỷỵỲÝỸỶỴ", "y" },
                {"ăằắẵẳặĂẰẮẴẲẶ", "a"},
                {"âầấẫẩậÂẦẤẪẨẬ", "a"},
                {"êềếễểệÊỀẾỄỂỆ", "e"},
                {"ồôỗốộổỒÔỖỐỘỔ", "o"},
                {"ơờớỡởợƠỜỚỠỞỢ", "o"},
                {"ừưữứửựỪƯỮỨỬỰ", "u"},
            };
        class myKey
        {
            char chMin, chMax;
            char[] tbl;
            Regex reg = new Regex("^[a-z0-9]+$", RegexOptions.IgnoreCase);

            public myKey()
            {
                tbl = makeTbl(out chMin, out chMax);
            }
            char[] makeTbl(out char outMin, out char outMax)
            {
                char min = 'á';
                char max = 'á';
                foreach (var p in dict)
                {
                    foreach (var ch in p.Key)
                    {
                        if (ch > max) { max = ch; }
                        if (ch < min) { min = ch; }
                    }
                }
                int d = max - min + 1;
                char[] tbl = new char[d];
                for (var ch = min; ch < max; ch++)
                {
                    tbl[ch - min] = ch;
                }
                foreach (var p in dict)
                {
                    foreach (var ch in p.Key)
                    {
                        tbl[ch - min] = p.Value[0];
                    }
                }
                outMin = min;
                outMax = max;
                return tbl;
            }
            public string genKey2(string value, out bool chk)
            {
                string ret = "";
                foreach (char ch in value)
                {
                    if (ch >= chMin && ch <= chMax)
                    {
                        ret += tbl[ch - chMin];
                    }
                    else
                    {
                        ret += ch;
                    }
                }
                chk = reg.IsMatch(ret);
                return ret.ToLower();
            }
        }

        public SrchResult Find(string txt)
        {
            int nRow;
            var arr = getWords(txt, out nRow);
            if (nRow == 0)
            {
                return null;
            }
            var recs = calcDiff(arr, nRow);

            var h = new HashSet<UInt64>();
            var parH = new HashSet<UInt64>();
            foreach (var res in recs)
            {
                h.Add(res.detail[0].titleId);
                foreach (var w in res.detail)
                {
                    if (parH.Contains(w.parId))
                    {
                        //dublicate
                    }
                    else
                    {
                        parH.Add(w.parId);
                    }
                }
            }
            //var titles = h.Select(titleId => getTitleInfo(titleId)).ToList();
            var items = parH.Select(parId => m_items[(int) parId]).ToArray();
            return new SrchResult()
            {
                //titles = titles,
                items = items,
                recs = recs
            };
        }
        
        private MyWord[][] getWords(string txt, out int nRow)
        {
            var reg = new Regex(@"[\w]+");
            var mc = reg.Matches(txt);
            var tDict = new Dictionary<string, UInt64>();
            foreach (Match m in mc)
            {
                if (tDict.ContainsKey(m.Value)){continue;}
                tDict.Add(m.Value, 0);
            }

            var arr = new MyWord[tDict.Count][];
            nRow = 0;
            foreach (var key in tDict.Keys)
            {
                var lst = new List<MyWord>();
                if (m_db.ContainsKey(key))
                {
                    arr[nRow++] = m_db[key].ToArray();
                }
            }

            return arr;
        }
            SrchRec[] calcDiff(MyWord[][] arr, int nRow)
        {
            int max_res = 1000;
            int max_d = 1000;

            var begin = Environment.TickCount;
            var res = new SrchRec[arr[0].Length];
            for (int i = 0; i < arr[0].Length; i++)
            {
                res[i] = new SrchRec() { path = new int[] { i }, d = 0 };
            }
            for (var row = 1; row < nRow; row++)
            {
                var lst = arr[row];
                var prevLst = arr[row - 1];
                var tmplRes = new List<int[]>();
                for (var j = 0; j < res.Length; j++)
                {
                    var prevD = res[j].d;
                    var prevI = res[j].path[row - 1];
                    var prevW = prevLst[prevI];
                    for (var k = 0; k < lst.Length; k++)
                    {
                        var curW = lst[k];
                        if (prevW.titleId == curW.titleId)
                        {
                            //var d = wordDiff(res[j].w, lst[k]);
                            var d = Convert.ToInt32(prevW.parId) - Convert.ToInt32(curW.parId);
                            d *= d < 0 ? -100 : 100;
                            d += Math.Abs(prevW.pos - curW.pos) * 10;
                            d += prevD;
                            tmplRes.Add(new int[] { j, k, d });
                        }
                    }
                }

                var h = new MyHeap<int[]>(tmplRes.ToArray(), (x, y) => x[2] - y[2]);
                var n = Math.Min(max_res, tmplRes.Count);
                var top = new SrchRec[n];
                for (int i = 0; i < n; i++)
                {
                    var t = h.PopMin();

                    //chk distance
                    if (t[2] > max_d)
                    {
                        Array.Resize(ref top, i);
                        break;
                    }

                    var newRec = new SrchRec() { d = t[2], path = new int[row + 1] };
                    res[t[0]].path.CopyTo(newRec.path, 0);
                    newRec.path[row] = t[1];
                    top[i] = newRec;
                }
                res = top;
            }
            var elapsed = Environment.TickCount - begin;
            Debug.WriteLine("calc diff {0}", elapsed);
            foreach (var item in res)
            {
                var row = 0;
                item.detail = item.path.Select(v => arr[row++][v]).ToList();
            }
            return res;
        }

        int wordDiff(MyWord w1, MyWord w2)
        {
            var a1 = new int[] {
                Convert.ToInt32(w1.titleId),
                Convert.ToInt32(w1.parId),
                w1.pos };
            var a2 = new int[] {
                Convert.ToInt32(w2.titleId),
                Convert.ToInt32(w2.parId),
                w2.pos };
            var c = new int[] { 1000, 100, 10 };
            int diff = 0;
            for (int i = 0; i < 3; i++)
            {
                diff += Math.Abs(a1[i] - a2[i]) * c[i];
            }
            return diff;
        }
    }
}
