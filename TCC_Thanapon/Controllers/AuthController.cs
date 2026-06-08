using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace TCC_Thanapon.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        [HttpPost("register")]
        public IActionResult Register([FromBody] signup input)
        {
            string i_username = input.username;
            string i_passwordsignin = input.passwordsignin;
            string i_confirmpassword = input.confirmpassword;
            if (i_passwordsignin != i_confirmpassword)
            {
                return Ok(new { status = "Error", message = "รหัสผ่านไม่ตรงกัน" });
            }
            var myClass = new Class.MyClassExecuteData();
            var myOption = new Class.MyClassOption();
            string users_code = myOption.Random_code("users", "users_code", 8);
            string users_name = i_username;
            string users_password = myOption.Encrypt(i_passwordsignin);
            string users_date_register = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            

            string sql = "";
            sql = $"SELECT users_code FROM users WHERE users_name = N{myOption.SQ(users_name)}";

            Boolean check_user =false;
            if (myClass.GetDataTable(sql).Rows.Count > 0)
            {
                check_user = true;
            }
            if (check_user)
            {
                return Ok(new { status = "Error", message = "ชื่อผู้ใช้นี้มีอยู่แล้ว" });
            }
            sql = "INSERT INTO users (users_code, users_name, users_password, users_date_register) ";
            sql += $"VALUES (N{myOption.SQ(users_code)}, N{myOption.SQ(users_name)}, N{myOption.SQ(users_password)}, {myOption.SQ(users_date_register)});";
            myClass.ExecuteNonQuery(sql);
            return Ok(new { status = "OK", message = "สมัครสมาชิกสำเร็จ" });
        }
        [HttpPost("login")]
        public IActionResult Login([FromBody] signin input)
        {
            string i_username = input.username;
            string i_password = input.password;
            if (string.IsNullOrEmpty(i_username) || string.IsNullOrEmpty(i_password))
            {
                return Ok(new { status = "Error", message = "กรุณากรอกข้อมูลให้ครบทุกช่อง" });
            }
            var myClass = new Class.MyClassExecuteData();
            var myOption = new Class.MyClassOption();
            string users_name = i_username;
            string users_password = myOption.Encrypt(i_password);
            string users_date_signin = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            string sql = "";
            sql = $"SELECT users_code FROM users WHERE (users_name = N{myOption.SQ(users_name)} AND users_password = N{myOption.SQ(users_password)})";
            var dt = myClass.GetDataTable(sql);
            string users_code = "";
            bool check_user = false;
            foreach (System.Data.DataRow row in dt.Rows)
            {
                check_user = true;
                users_code = row["users_code"].ToString();
            }
            if (!check_user)
            {
                return Ok(new { status = "Error", message = "ชื่อผู้ใช้หรือรหัสผ่านไม่ถูกต้อง" });
            }

            var claims = new[] {
            new Claim(ClaimTypes.Name, users_code)
        };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("7bM9xW2vR5zK4pQ8sJ1mN3tY6cBX5fH2"));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: "YourAppBackend",
                audience: "YourAppFrontend",
                claims: claims,
                expires: DateTime.UtcNow.AddDays(1),
                signingCredentials: creds
            );

            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);


            sql = $"UPDATE users SET users_date_signin = N{myOption.SQ(users_date_signin)} WHERE users_code = N{myOption.SQ(users_code)}";
            myClass.ExecuteNonQuery(sql);
            return Ok(new { status = "OK", token = tokenString });
        }
        [HttpPost("token")]
        public IActionResult Token()
        {
            var authHeader = Request.Headers["Authorization"].ToString();

            if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
            {
                return Ok(new { status = "Error" });
            }
            var token = authHeader.Substring("Bearer ".Length).Trim();

            try
            {
                // 2. ทำการ Validate ความถูกต้อง
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.UTF8.GetBytes("7bM9xW2vR5zK4pQ8sJ1mN3tY6cBX5fH2");

                tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidIssuer = "YourAppBackend",
                    ValidateAudience = true,
                    ValidAudience = "YourAppFrontend",
                    ClockSkew = TimeSpan.Zero
                }, out SecurityToken validatedToken);

                var jwtToken = (JwtSecurityToken)validatedToken;
                var users_code = jwtToken.Claims.First(x => x.Type == ClaimTypes.Name).Value;
                var myClass = new Class.MyClassExecuteData();
                var myOption = new Class.MyClassOption();
                string users_name = "";
                string sql = "";
                sql = $"SELECT users_name FROM users WHERE users_code = N{myOption.SQ(users_code)}";
                var dt = myClass.GetDataTable(sql);
                bool check_user = false;
                foreach (System.Data.DataRow row in dt.Rows)
                {
                    check_user = true;
                    users_name = row["users_name"].ToString();
                }
                if (!check_user)
                {
                    return Ok(new { status = "Error" });
                }
                return Ok(new { status = "OK", username = users_name });
            }
            catch
            {
                return Ok(new { status = "Error" });
            }
        }
    }
    public class signup
    {
        [Required]
        public required string username { get; set; }
        [Required]
        public required string passwordsignin { get; set; }
        [Required]
        public required string confirmpassword { get; set; }
    }
    public class signin
    {
        [Required]
        public required string username { get; set; }
        [Required]
        public required string password { get; set; }
    }
    public class set_token
    {
        [Required]
        public required string v_token { get; set; }
    }



}
