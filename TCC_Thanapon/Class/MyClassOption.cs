using Microsoft.VisualBasic.FileIO;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Http;

namespace TCC_Thanapon.Class
{
    public class MyClassOption
    {
        public string SQ(string sql)
        {
            if (sql == null)
            {
                sql = "";
            }
            return "'" + sql.Replace("'", "''") + "'";
        }
        public string Random_code(string _FROM, string _WHERE, int number)
        {
            string _allowedChars = "123456789";
            Random randNum = new Random();
            char[] chars = new char[number];
            int allowedCharCount = _allowedChars.Length;
            string _code;

            do
            {
                for (int i = 0; i < number; i++)
                {
                    // ใช้ randNum.Next(max) แทนการคำนวณ NextDouble() สุ่ม Index ได้แม่นยำและสั้นกว่า
                    chars[i] = _allowedChars[randNum.Next(allowedCharCount)];
                }

                _code = new string(chars);

            } while (chk_code(_FROM, _WHERE, _code) == true); // ถ้าซ้ำ (true) ให้สุ่มใหม่

            return _code;
        }
        public bool chk_code(string _FROM, string _WHERE, string _code)
        {
            var myClass = new Class.MyClassExecuteData();
            var myOption = new Class.MyClassOption();
            string sql = $"SELECT {myOption.SQ(_WHERE)} FROM {_FROM} WHERE {myOption.SQ(_WHERE)} = N{myOption.SQ(_code)}";
            Boolean check_user = false;
            if (myClass.GetDataTable(sql).Rows.Count > 0)
            {
                check_user = true;
            }
            return check_user;
        }

        public string Encrypt(string toEncrypt)
        {
            if (string.IsNullOrEmpty(toEncrypt)) return "";

            bool useHashing = true;
            byte[] keyArray;
            byte[] toEncryptArray = Encoding.UTF8.GetBytes(toEncrypt);
            string key = "1001";

            if (useHashing)
            {
                using (MD5CryptoServiceProvider hashmd5 = new MD5CryptoServiceProvider())
                {
                    keyArray = hashmd5.ComputeHash(Encoding.UTF8.GetBytes(key));
                    hashmd5.Clear();
                }
            }
            else
            {
                keyArray = Encoding.UTF8.GetBytes(key);
            }

            using (TripleDESCryptoServiceProvider tdes = new TripleDESCryptoServiceProvider())
            {
                tdes.Key = keyArray;
                tdes.Mode = CipherMode.ECB;
                tdes.Padding = PaddingMode.PKCS7;

                using (ICryptoTransform cTransform = tdes.CreateEncryptor())
                {
                    byte[] resultArray = cTransform.TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length);
                    tdes.Clear();

                    // เข้ารหัสเสร็จ -> เป็น Base64 String -> เปลี่ยนจาก / เป็น %2 เพื่อความปลอดภัยบน URL
                    return Convert.ToBase64String(resultArray, 0, resultArray.Length).Replace("+", "1981").
            Replace("/", "1982").
            Replace(".", "1983").
            Replace("-", "1983").
            Replace("*", "1984");
                }
            }
        }

        public string Decrypt(string cipherString)
        {
            if (string.IsNullOrEmpty(cipherString)) return "";

            try
            {
                bool useHashing = true;
                byte[] keyArray;
                string key = "1001";

                if (useHashing)
                {
                    using (MD5CryptoServiceProvider hashmd5 = new MD5CryptoServiceProvider())
                    {
                        keyArray = hashmd5.ComputeHash(Encoding.UTF8.GetBytes(key));
                        hashmd5.Clear();
                    }
                }
                else
                {
                    keyArray = Encoding.UTF8.GetBytes(key);
                }
                cipherString = cipherString.Replace("%2", "/").
                                                                        Replace("1981", "+").
                                                                        Replace("1982", "/").
                                                                        Replace("1983", ".").
                                                                        Replace("1983", "-").
                                                                        Replace("1984", "*");
                // 1. เปลี่ยน %2 กลับมาเป็น / และจัดการช่องว่างก่อนถอด Base64
                byte[] toEncryptArray = DecodeBase64(cipherString);

                using (TripleDESCryptoServiceProvider tdes = new TripleDESCryptoServiceProvider())
                {
                    tdes.Key = keyArray;
                    tdes.Mode = CipherMode.ECB;
                    tdes.Padding = PaddingMode.PKCS7;

                    using (ICryptoTransform cTransform = tdes.CreateDecryptor())
                    {
                        byte[] resultArray = cTransform.TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length);
                        tdes.Clear();
                        return Encoding.UTF8.GetString(resultArray);
                    }
                }
            }
            catch (Exception ex)
            {
                // แนะนำให้ใส่ตัวแปร ex ไว้ เผื่อคุณอยากใช้ดักตรวจสอบดูว่ามัน Error เพราะอะไรในตอนเขียนโค้ดครับ
                return "";
            }
        }

        // เปลี่ยนชื่อเพื่อให้ตรงกับหน้าที่จริง (เพราะมันรับสตริงเข้ามาแปลงคืนเป็น byte[])
        public byte[] DecodeBase64(string data)
        {
            // เปลี่ยน %2 กลับเป็น / และเปลี่ยนช่องว่างเป็น + ตามมาตรฐาน Base64 ของ URL
            string s = data.Trim().Replace("%2", "/").Replace(" ", "+");

            // เติมพาร์ทชันด้วยเครื่องหมาย = ให้ครบล็อคตัวคูณ 4 ถ้าจำเป็น
            if (s.Length % 4 > 0)
            {
                s = s.PadRight(s.Length + 4 - (s.Length % 4), '=');
            }

            return Convert.FromBase64String(s);
        }
        public void setCookie(HttpContext httpContext, string CookieKeyName, string cookieValue)
        {
            // ทำการเข้ารหัสป้องกัน XSS (เหมือน HtmlEncode ของเดิม)
            string encodedValue = WebUtility.HtmlEncode(cookieValue);

            // ตั้งค่าตัวเลือกของ Cookie (เช่น อายุการใช้งาน)
            CookieOptions options = new CookieOptions
            {
                Expires = DateTime.Now.AddDays(7), // กำหนดให้อยู่ได้ 7 วัน (ปรับเปลี่ยนได้ตามต้องการ)
                HttpOnly = true, // เพื่อความปลอดภัย ป้องกันไม่ให้ JavaScript ฝั่ง Client แอบมาอ่านค่าได้
                Secure = true    // ทำงานเฉพาะบน https เท่านั้น
            };

            httpContext.Response.Cookies.Append(CookieKeyName, encodedValue, options);
        }

        public string getCookie(HttpContext httpContext, string CookieKeyName)
        {
            try
            {
                // ดึงค่า Cookie จาก Request
                string cookieValue = httpContext.Request.Cookies[CookieKeyName];

                if (string.IsNullOrEmpty(cookieValue))
                {
                    return "";
                }

                return WebUtility.HtmlDecode(cookieValue);
            }
            catch (Exception)
            {
                return "";
            }
        }

        public string delCookie(HttpContext httpContext, string CookieKeyName)
        {
            try
            {
                httpContext.Response.Cookies.Delete(CookieKeyName);
                return "OK";
            }
            catch (Exception)
            {
                return "";
            }
        }
    }

}
