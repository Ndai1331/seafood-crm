# Seafood CRM — tài liệu nghiệp vụ

Nguồn gốc: `seeds/data-sample.xlsx` (8 sheet). Tài liệu này là ngữ cảnh đọc lại khi làm UI/API.

## Luồng nghiệp vụ

Master data → Nhập nguyên liệu → Đưa vào sản xuất → Tồn kho & sắp xếp giấy → Báo giá/HĐ → Đơn xuất khẩu → Thanh toán & Deposit.

Tiền gốc: **USD**. Quy đổi khối lượng xuất khẩu: `1 lb = 0.45359237 kg` (`api/Core/Helper/WeightUnits.cs`).

## Mục lục sheet → docs → route → API

| Sheet Excel | Docs | Route UI | API |
|---|---|---|---|
| Thông tin tổng hợp | [01-master-data.md](01-master-data.md) | `/master-data/catalog`, `/master-data/products`, `/master-data/customers`, `/master-data/payment-terms` | `GET/POST/DELETE /api/seafood/catalog\|products\|customers\|payment-terms` |
| Theo dõi hàng nhập hằng ngày | [02-inbound.md](02-inbound.md) | `/inbound` | `/api/seafood/inbound` |
| đưa vào sản xuất | [03-production.md](03-production.md) | `/production` | `/api/seafood/production` |
| Ví dụ sắp xếp giấy | [04-certificate-allocation.md](04-certificate-allocation.md) | `/inventory`, `/inventory/allocate` | `/api/seafood/inventory`, `/inventory/allocate` |
| Các task chính cần có | [05-reports-quotes.md](05-reports-quotes.md) | `/dashboard`, `/reports`, `/export/quotes` | `/api/seafood/dashboard`, `/reports`, `/quotes` |
| Xuất khẩu - đơn hàng | [06-export-orders.md](06-export-orders.md) | `/export/orders` | `/api/seafood/orders` |
| Xuất khẩu - PAYMENT TRACKING | [07-payments.md](07-payments.md) | `/finance/payments` | `/api/seafood/payments` |
| Xuất khẩu _ DEPOSIT | [08-deposits.md](08-deposits.md) | `/finance/deposits` | `/api/seafood/deposits` |

Tổng quan hệ thống: [00-overview.md](00-overview.md).

## Role

`SUPER_ADMIN`, `ADMIN`, `PURCHASING`, `PRODUCTION`, `WAREHOUSE`, `SALES`, `ACCOUNTING`, `VIEWER`.

Menu đồng bộ `api/Core/Const/Permissions.cs` và `ui/.../PageRegistry.cs`.
