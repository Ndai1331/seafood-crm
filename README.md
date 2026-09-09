# Seafood CRM

Monorepo .NET 9: Blazor Server UI + Web API, PostgreSQL. CRM nhà máy chế biến cá ngừ (nhập kho, sản xuất, tồn kho/chứng từ, xuất khẩu, thanh toán, deposit).

## Chạy bằng Docker

```bash
cp .env.example .env
./docker-up.sh
```

- UI: http://localhost:8080
- API Swagger: http://localhost:5093/swagger
- Tài khoản seed: `admin` / `Admin@123` (đổi trong `.env`)

## Chạy local (không Docker UI/API)

1. Postgres 16 trên `localhost:5432`, database `seafood_crm`, user/password `seafood`.
2. API: `dotnet run --project api/WebApi/WebApi.csproj` (port 5093)
3. UI: `dotnet run --project ui/src/BootstrapBlazor.Server/BootstrapBlazor.Server.csproj` (port 5053)

## Phân quyền

Role: `SUPER_ADMIN`, `ADMIN`, `PURCHASING`, `PRODUCTION`, `WAREHOUSE`, `SALES`, `ACCOUNTING`, `VIEWER`.

Menu đồng bộ `api/Core/Const/Permissions.cs` và `ui/.../PageRegistry.cs`.

## Master data

Seed từ `seeds/data-sample.xlsx`: loài cá ngừ, dạng NL WR/GG/DWT, size, cảm quan, đánh bắt, cấp đông, chứng từ EUCC/COA/MSC, thị trường USA/EU/Khác, SKU thành phẩm, khách hàng, điều khoản TT/LC.

Quy đổi khối lượng xuất khẩu: `1 lb = 0.45359237 kg`.
