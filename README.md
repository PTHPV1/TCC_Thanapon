# TCC_Thanapon

โปรเจกต์นี้เป็นเว็บแอป ASP.NET Core MVC สำหรับระบบสมัครสมาชิก, เข้าสู่ระบบ, และตรวจสอบตัวตนผู้ใช้ด้วย JWT โดยหน้าเว็บฝั่งผู้ใช้เรียก API ผ่าน JavaScript และเก็บ token ไว้ใน `localStorage`

## ภาพรวมการทำงาน

ระบบนี้แบ่งการทำงานออกเป็น 2 ส่วนหลัก

- ฝั่งหน้าเว็บ (`Views`) ใช้ Razor View + JavaScript สำหรับแสดงฟอร์มและเรียก API
- ฝั่งหลังบ้าน (`Controllers` + `Class`) รับคำขอจากหน้าเว็บ, ตรวจสอบข้อมูล, ติดต่อฐานข้อมูล SQL Server, และออก JWT token

ลำดับการทำงานหลักมีดังนี้

1. ผู้ใช้สมัครสมาชิกที่หน้า `/register`
2. หน้าเว็บส่งข้อมูลไปที่ `POST /api/auth/register`
3. ระบบตรวจสอบชื่อผู้ใช้ซ้ำ, เข้ารหัสรหัสผ่าน, แล้วบันทึกลงตาราง `users`
4. ผู้ใช้เข้าสู่ระบบที่หน้า `/`
5. หน้าเว็บส่งข้อมูลไปที่ `POST /api/auth/login`
6. หากข้อมูลถูกต้อง ระบบจะสร้าง JWT แล้วส่งกลับมาให้หน้าเว็บ
7. หน้า `/user` จะอ่าน token จาก `localStorage` แล้วส่งไปตรวจสอบกับ `POST /api/auth/token`
8. ถ้า token ถูกต้อง ระบบจะคืนชื่อผู้ใช้กลับมาเพื่อแสดงบนหน้าเว็บ

## เทคโนโลยีที่ใช้

- .NET 9
- ASP.NET Core MVC
- SQL Server Express
- jQuery
- Atlantis admin template ในโฟลเดอร์ `wwwroot/assets`
- JWT สำหรับยืนยันตัวตน

## โครงสร้างโปรเจกต์

### `TCC_Thanapon/Program.cs`

เป็นจุดเริ่มต้นของแอป ทำหน้าที่ตั้งค่า service และ route ของ ASP.NET Core เช่น

- `AddControllersWithViews()` เปิดใช้งาน MVC
- `UseHttpsRedirection()` บังคับ redirect ไป HTTPS
- `MapControllerRoute()` กำหนด route หลักของระบบ
- route เริ่มต้นคือ `{controller=Home}/{action=Index}/{id?}`

### `TCC_Thanapon/Controllers/HomeController.cs`

controller นี้ใช้สำหรับเปิดหน้าเว็บ

- `Index()` แสดงหน้าเข้าสู่ระบบ
- `Register()` แสดงหน้าสมัครสมาชิก
- `User()` แสดงหน้าผู้ใช้หลังเข้าสู่ระบบ
- `Error()` แสดงหน้า error มาตรฐานของ MVC

### `TCC_Thanapon/Controllers/AuthController.cs`

controller นี้เป็น API หลักของระบบ authentication

#### `POST /api/auth/register`

รับข้อมูลสมัครสมาชิกจากหน้า `Register.cshtml`

- ตรวจว่ารหัสผ่านและยืนยันรหัสผ่านตรงกันหรือไม่
- ตรวจว่าชื่อผู้ใช้มีอยู่แล้วหรือไม่
- สุ่ม `users_code`
- เข้ารหัสรหัสผ่านก่อนบันทึก
- บันทึกข้อมูลลงตาราง `users`

#### `POST /api/auth/login`

รับข้อมูลเข้าสู่ระบบจากหน้า `Index.cshtml`

- ตรวจว่ากรอกข้อมูลครบหรือไม่
- เข้ารหัสรหัสผ่านเพื่อนำไปเทียบกับฐานข้อมูล
- ค้นหาผู้ใช้จากตาราง `users`
- หากพบผู้ใช้ จะสร้าง JWT token และอัปเดตเวลาเข้าสู่ระบบล่าสุด
- ส่ง token กลับไปยังหน้าเว็บ

#### `POST /api/auth/token`

ใช้ตรวจสอบ token จากหน้า `User.cshtml`

- อ่านค่า `Authorization: Bearer <token>`
- ตรวจสอบความถูกต้องของ JWT
- ดึง `users_code` จาก claim
- ค้นหาชื่อผู้ใช้จากฐานข้อมูล
- ส่งชื่อผู้ใช้กลับไปแสดงผล

### `TCC_Thanapon/Class/MyClassExecuteData.cs`

คลาสนี้ใช้ติดต่อฐานข้อมูล SQL Server โดยตรง

- `GetDataTable(string sql)` ใช้ดึงข้อมูลจากฐานข้อมูลแล้วคืนค่าเป็น `DataTable`
- `ExecuteNonQuery(string sql)` ใช้กับคำสั่ง `INSERT`, `UPDATE`, หรือคำสั่งที่ไม่ต้องคืนผลลัพธ์

ปัจจุบัน connection string ถูกเขียนไว้ในคลาสโดยตรง:

```text
Data Source=.\SQLEXPRESS;Initial Catalog=TCC_TEST;TrustServerCertificate=True;Integrated Security=True
```

ดังนั้นถ้าเครื่องที่จะรันโปรเจกต์มีชื่อ instance หรือชื่อฐานข้อมูลไม่ตรงกัน ต้องแก้ค่าที่ไฟล์นี้ก่อน

### `TCC_Thanapon/Class/MyClassOption.cs`

คลาส utility ที่รวมฟังก์ชันช่วยเหลือหลายอย่าง เช่น

- `SQ()` ครอบ string ให้พร้อมใช้ใน SQL
- `Random_code()` สุ่มรหัสสำหรับ `users_code`
- `chk_code()` เช็กว่ารหัสที่สุ่มซ้ำในฐานข้อมูลหรือไม่
- `Encrypt()` เข้ารหัสรหัสผ่านก่อนบันทึกและก่อนตรวจสอบตอน login
- `Decrypt()` ถอดรหัสข้อมูลที่เคยเข้ารหัส
- `DecodeBase64()` เตรียมข้อมูลก่อนถอดรหัส
- `setCookie()`, `getCookie()`, `delCookie()` สำหรับจัดการ cookie

### `TCC_Thanapon/Views/Home/Index.cshtml`

หน้าเข้าสู่ระบบ

- รับ `username` และ `password`
- เมื่อกดปุ่มเข้าสู่ระบบ จะเรียก `POST /api/auth/login`
- ถ้า login สำเร็จ จะเก็บ token ไว้ใน `localStorage` ชื่อ `JwtToken`
- จากนั้น redirect ไปหน้า `/user`

### `TCC_Thanapon/Views/Home/Register.cshtml`

หน้าสมัครสมาชิก

- รับ `username`, `password`, และ `confirm password`
- ตรวจสอบข้อมูลเบื้องต้นที่ฝั่งหน้าเว็บ
- เรียก `POST /api/auth/register`
- ถ้าสมัครสำเร็จ จะ redirect กลับไปหน้า login

### `TCC_Thanapon/Views/Home/User.cshtml`

หน้าสำหรับผู้ใช้ที่เข้าสู่ระบบแล้ว

- อ่าน token จาก `localStorage`
- ถ้าไม่มี token จะเด้งกลับหน้า login
- ถ้ามี token จะเรียก `POST /api/auth/token` เพื่อตรวจสอบสิทธิ์
- หาก token ถูกต้อง จะแสดงชื่อผู้ใช้บนหน้าจอ
- ปุ่ม logout จะลบ token ออกจาก `localStorage` แล้วกลับหน้าแรก

### `TCC_Thanapon/Views/Shared/_Layout.cshtml`

layout กลางของทุกหน้าในระบบ

- โหลด CSS จาก `wwwroot/assets/css`
- โหลด JavaScript หลัก เช่น jQuery, Bootstrap, SweetAlert, Atlantis
- เรียก `@RenderBody()` เพื่อแสดงเนื้อหาของแต่ละหน้า
- เรียก `@RenderSectionAsync("Scripts")` เพื่อให้แต่ละหน้าแทรก script เฉพาะของตัวเองได้

### `TCC_Thanapon/wwwroot/assets`

เก็บ static files ทั้งหมดของหน้าเว็บ เช่น

- CSS
- JavaScript
- fonts
- plugins ของ template Atlantis

โฟลเดอร์นี้ทำให้หน้า login, register, และ user มีหน้าตาและ interaction พร้อมใช้งาน

## เส้นทางหน้าเว็บและ API

### หน้าเว็บ

- `GET /` หน้าเข้าสู่ระบบ
- `GET /register` หน้าสมัครสมาชิก
- `GET /user` หน้าผู้ใช้

### API

- `POST /api/auth/register` สมัครสมาชิก
- `POST /api/auth/login` เข้าสู่ระบบ
- `POST /api/auth/token` ตรวจสอบ token และดึงข้อมูลผู้ใช้

## โครงสร้างฐานข้อมูลที่ระบบคาดหวัง

จากโค้ดใน `AuthController` ระบบคาดหวังว่าจะมีตาราง `users` อย่างน้อยด้วยคอลัมน์ต่อไปนี้

- `users_code`
- `users_name`
- `users_password`
- `users_date_register`
- `users_date_signin`

ตัวอย่างโครงสร้างเบื้องต้น

```sql
CREATE TABLE [dbo].[users](
	[users_id] [int] IDENTITY(1,1) NOT NULL,
	[users_code] [nvarchar](8) NULL,
	[users_name] [nvarchar](250) NULL,
	[users_password] [nvarchar](50) NULL,
	[users_date_register] [datetime] NULL,
	[users_date_signin] [datetime] NULL
) ON [PRIMARY]
```

## วิธีรันโปรเจกต์

1. ติดตั้ง .NET 9 SDK
2. ติดตั้ง SQL Server Express หรือปรับ connection string ให้ตรงกับเครื่องที่ใช้งาน
3. สร้างฐานข้อมูล `TCC_TEST`
4. สร้างตาราง `users` ตามโครงสร้างด้านบน
5. ตรวจสอบค่า connection string ใน `TCC_Thanapon/Class/MyClassExecuteData.cs`
6. รันคำสั่ง:

```bash
dotnet restore
dotnet run --project TCC_Thanapon/TCC_Thanapon.csproj
```

ค่า URL ตอนรันในโหมดพัฒนาอ้างอิงจาก `Properties/launchSettings.json`

- `http://localhost:5206`
- `https://localhost:7204`

## หมายเหตุ

- โปรเจกต์นี้ใช้การเขียน SQL string ตรงในโค้ด ยังไม่ได้ใช้ ORM จริงจังแม้จะอ้างอิงแพ็กเกจ `Microsoft.EntityFrameworkCore.SqlServer`
- connection string และ JWT secret ยังถูกเขียนไว้ใน source code จึงเหมาะกับงานทดลองหรือใช้ในเครื่องพัฒนาเป็นหลัก
- token ถูกเก็บไว้ใน `localStorage` เพื่อให้หน้า `User` อ่านไปตรวจสอบต่อกับ API
