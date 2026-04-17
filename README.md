# Glowify Skincare - E-Commerce Platform

Glowify is a full-stack e-commerce web application built with ASP.NET Core MVC. It is designed to provide a complete and secure online shopping experience, featuring a dynamic shopping cart, integrated payment gateway, and a comprehensive admin management panel.

## Features

* **Product & Category Management:** Full CRUD operations for products and categories via the admin dashboard.
* **Shopping Cart & Checkout:** Database-driven shopping cart with real-time total and shipping calculations.
* **Secure Payment Integration:** Integrated with the Iyzico (Iyzipay) API for processing credit card payments securely.
* **Authentication & Authorization:** Role-based access control (Admin/Customer) using ASP.NET Core Identity. Supports standard login and **Google Account Login** (OAuth 2.0).
* **Order Management:** Order tracking, purchase history, and status updates for both customers and administrators.
* **Discount System:** Dynamic coupon code infrastructure based on minimum cart totals.

## Tech Stack

* **Backend:** C#, ASP.NET Core MVC
* **ORM:** Entity Framework Core (Code-First Approach)
* **Database:** Microsoft SQL Server (MSSQL)
* **Frontend:** HTML5, CSS3, JavaScript, Bootstrap 5
* **Identity & Security:** ASP.NET Core Identity, Google OAuth 2.0
* **External APIs:** Iyzico API (Payment Processing)

## Local Setup & Installation

To run this project locally, follow these steps:

1. **Clone the repository:**
   ```bash
   git clone [https://github.com/yourusername/Glowify.git](https://github.com/yourusername/Glowify.git)
   ```

2. **Configure App Settings:**
   Open the `appsettings.json` file and update the required credentials. Make sure to replace the placeholder values with your actual keys and database information:

   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Server=YOUR_SERVER_NAME;Database=YOUR_DATABASE_NAME;User Id=YOUR_DB_USER;Password=YOUR_DB_PASSWORD;Encrypt=False;MultipleActiveResultSets=True;"
     },
     "IyzicoOptions": {
       "ApiKey": "YOUR_IYZICO_API_KEY",
       "SecretKey": "YOUR_IYZICO_SECRET_KEY",
       "BaseUrl": "[https://sandbox-api.iyzipay.com](https://sandbox-api.iyzipay.com)",
       "CallbackUrl": "https://localhost:YOUR_PORT/Customer/Cart/CallBack"
     },
     "EmailSettings": {
       "Email": "YOUR_EMAIL_ADDRESS",
       "Password": "YOUR_EMAIL_APP_PASSWORD"
     },
     "Authentication": {
       "Google": {
         "ClientId": "YOUR_GOOGLE_CLIENT_ID",
         "ClientSecret": "YOUR_GOOGLE_CLIENT_SECRET"
       }
     }
   }
   ```

3. **Apply Migrations:**
   Open the Package Manager Console (PMC) in Visual Studio and run the following command to create the database schema:
   ```powershell
   Update-Database
   ```

4. **Run the Application:**
   Press `F5` in Visual Studio or run `dotnet run` in your terminal.

## Live Preview

The application is currently deployed and live at: 
[Glowify Skincare Live Demo](https://glowify.runasp.net)
