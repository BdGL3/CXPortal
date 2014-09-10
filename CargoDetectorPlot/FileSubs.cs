using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using System.IO;

namespace L3.Cargo.DetectorPlot
{
    class FileSubs
    {
        public FileSubs()
        {
        }
        //=============================================================
        #region PXE files
        public string SavePXEFile(int savefilecnt, UInt32 NumofSavedLines, byte[] saveddata,bool chk,string fn)
        {//spatial mode
            string SavedFileName = "";
            string pp = "";
            float[] pxedata;
            try
            {
                string dir = ConfigurationManager.AppSettings["SPModeFname"];
                UInt32 height = (UInt32)saveddata.Length / NumofSavedLines;
                if (!Directory.Exists(dir))
                    Directory.CreateDirectory(dir);
                if (chk == true)
                {
                    string fname = dir + "\\test" + savefilecnt.ToString() + ".pxe";
                    CheckSavedFileName(fname, ref pp, 0, "");
                }
                else
                    pp = fn;
                PxeAccess pa = new PxeAccess();
                pa.CreatePXE(pp);
                pa.CreatePXEHeader(1, NumofSavedLines, height);
                //convert saveddata to float array 
                pxedata = new float[saveddata.Length];
                for (int i = 0; i < saveddata.Length; i++)
                {
                    pxedata[i] = (float)saveddata[i];
                }
                pa.WriteDataLines(1, pxedata, NumofSavedLines);
                pa.ClosePXE();
                SavedFileName = pp;
            }
            catch
            {
                SavedFileName = "";
            }
            return SavedFileName;
        }

        private void CheckSavedFileName(string fname, ref string newfname,int op,string extra)
        {
            int num = 0;
            string ss = "";
            string a = "";
            string therest = "";
            string newname = "";
            string name = System.IO.Path.GetFileNameWithoutExtension(fname);
            for (int k = name.Length; k > 0; k--)
            {
                byte l = (byte)name.ElementAt(k - 1);
                if (l > 47 && l < 58)//number
                {
                    a = (char)l + a;
                }
                else
                {
                    therest = name.Substring(0, k);
                    break;
                }
            }
            num = Convert.ToInt32(a);
            if (op==0)
               newname = System.IO.Path.GetDirectoryName(fname) + "\\test" + (num + 1).ToString() + ".pxe";
            else if (op == 1)
            {
                newname = System.IO.Path.GetDirectoryName(fname) + "\\" +extra +"test" + (num + 1).ToString() + ".pbn";
            }
            if (File.Exists(newname) == true)
            {
                CheckSavedFileName(newname, ref ss, op, extra);//recursion
                newfname = ss;
            }
            else
                newfname = newname;
        }

        public string SaveTimeData(byte[] inarr, int lnum,int [] sdet,bool chk,string nm)
        {
            string SavedFileName = "";
            int nn = sdet.Length;
            //=============== extra cleaning
            float[] detinfo = new float[lnum + 1];
            for (int i = 0; i < lnum + 1; i++)
                detinfo[i] = 0;
            //============================================================

            try
            {
                detinfo[0] = (float)nn;
                for (int i = 1; i < nn + 1; i++)
                    detinfo[i] = (float)sdet[i - 1];
                string pp = "";
                float[] pxedata;
                string dir = ConfigurationManager.AppSettings["TimeModeFname"];
                UInt32 height = (UInt32)(inarr.Length / lnum);
                if (!Directory.Exists(dir))
                    Directory.CreateDirectory(dir);
                if (chk == true)
                {
                    string fname = dir + "\\test0.pxe";
                    CheckSavedFileName(fname, ref pp, 0, "");
                }
                else
                    pp = nm;
                PxeAccess pa = new PxeAccess();
                pa.CreatePXE(pp);
                pa.CreatePXEHeader(1, (uint)lnum, height);
                pxedata = new float[inarr.Length];
                for (int i = 0; i < inarr.Length; i++)
                {
                    pxedata[i] = (float)inarr[i];
                }
                pa.WriteDataLines(1, pxedata, (uint)lnum);
                int rt = pa.WriteRefSample(1, detinfo);
                pa.ClosePXE();
                SavedFileName = pp;
            }
            catch
            {
                SavedFileName = "";
            }
            return SavedFileName;
        }
        #endregion
        #region Binary files
        public void SaveComment(string Comment, string fname)
        {
            try
            {
                if (File.Exists(fname))
                    File.Delete(fname);  
                using (System.IO.StreamWriter writer = new System.IO.StreamWriter(fname, true))
                {
                    writer.Write(Comment);
                    writer.Close(); 
                }

            }
            catch { }

        }
        public string ReadComment(string fname)
        {
            string strret = "";
            if (File.Exists(fname))
            {
                try
                {
                    using (System.IO.StreamReader rd = new System.IO.StreamReader(fname))
                    {
                        strret = rd.ReadToEnd();
                        rd.Close();
                    }
                }
                catch { strret = ""; }
            }
            return strret;
        }
        public string SaveBinary(byte[] data,string comment,int NumofRuns,int mode ,bool chk,string fn)
        {//binary mode
            string SavedFileName = "";
            string pp = "";
            string dir = "";
            string fname = "";
            string extra = data.Length.ToString() + "X" + NumofRuns.ToString();   
            try
            {
                if (mode == 0)
                 dir = ConfigurationManager.AppSettings["SPModeFname"];
                else
                 dir = ConfigurationManager.AppSettings["TimeModeFname"];
                if (!Directory.Exists(dir))
                    Directory.CreateDirectory(dir);
                if (chk == true)
                {
                    fname = dir + "\\test0" + ".pbn";
                    CheckSavedFileName(fname, ref pp, 1, extra);
                }
                else
                    pp = fn;
                using (FileStream _FS = new FileStream(pp, FileMode.CreateNew, FileAccess.Write, FileShare.None))
                {
                    _FS.Write(data, 0, data.Length);
                    SavedFileName = pp;
                }
                if (comment !="")
                {
                  fname=pp+".txt";
                  SaveComment(comment, fname);
                }
            }
            catch
            {
                SavedFileName = "";
            }
            return SavedFileName;
        }
        public byte[] ReadBinary(string fname)
        {
            byte[] data = new byte[0];
            if (File.Exists(fname))
            {
                FileInfo fi = new FileInfo(fname);
                data = new byte[fi.Length];
                using (FileStream fs = new FileStream(fname, FileMode.Open, FileAccess.Read))
                {
                    fs.Read(data, 0, data.Length);
                }
             }
            return data;
        }

        public int[] GetReadingsCount(string fname)
        {
            int [] num = new int[2];
            string s1, s2, s3;
            int cnt = 0;
            int pos;
            byte l;
            if (fname != "")
            {
                s1 = System.IO.Path.GetFileName(fname);
                s2 = "";
                do
                {
                    l = (byte)s1.ElementAt(cnt);
                    s3 = s1.ElementAt(cnt).ToString();
                    s2 += s3;
                    cnt++;
                }
                while (l > 47 && l < 58);
                num[0]= Convert.ToInt32( s2.Substring(0, cnt - 1));
                pos = s1.IndexOf("X")+1;
                if (pos == 1)
                {
                    num[0] = 0;
                    num[1] = 0;
                    return num;
                }
                else
                {
                    s2 = "";
                    s3 = "";
                    cnt = pos;
                    do
                    {
                        l = (byte)s1.ElementAt(cnt);
                        s3 = s1.ElementAt(cnt).ToString();
                        s2 += s3;
                        cnt++;
                    }
                    while (l > 47 && l < 58);
                    num[1] = Convert.ToInt32(s2.Substring(0, s2.Length  - 1));
                }
            }
            return num;
        }
        public string SavePXEAN(string fname, uint NumofSavedLines,byte [] dt,string comment )
        {
            float[] pxedata;
            string sret = "";
            string CommentFileName;
            try
            {
                UInt32 height = (UInt32)(dt.Length / NumofSavedLines);
                PxeAccess pa = new PxeAccess();
                pa.CreatePXE(fname);
                pa.CreatePXEHeader(1, NumofSavedLines, height);
                //convert data to float array 
                pxedata = new float[dt.Length];
                for (int i = 0; i < dt.Length; i++)
                {
                    pxedata[i] = (float)dt[i];
                }
                pa.WriteDataLines(1, pxedata, NumofSavedLines);
                pa.ClosePXE();
                if (comment != "")
                {
                    CommentFileName = fname + ".txt";
                    SaveComment(comment, CommentFileName);
                }
                sret = fname;
            }
            catch { }
            return sret;
        }
        public string SaveBinAn(string fname, byte[] dt,string comment)
        {
            string sret = "";
            string CommentFileName = "";
            try
            {
                using (FileStream _FS = new FileStream(fname, FileMode.CreateNew, FileAccess.Write, FileShare.None))
                {
                    _FS.Write(dt, 0, dt.Length);
                    sret = fname;
                }
                if (comment != "")
                {
                    CommentFileName = fname + ".txt";
                    SaveComment(comment, CommentFileName);
                }
            }
            catch { }
            return sret;
        }
        #endregion
    }
}
