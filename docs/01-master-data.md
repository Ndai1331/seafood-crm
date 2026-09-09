# Master data — sheet «Thông tin tổng hợp»

## Lookup (CatalogLookup)

| Category | Code / giá trị | Ý nghĩa |
|---|---|---|
| FishSpecies | TUNA | Cá ngừ |
| FishForm | WR / GG / DWT | Nguyên con / bỏ nội tạng-vây / bỏ đầu |
| SizeGrade | 10-20kg, 20kg up, 20-30kg, 30kg Up | Size nguyên liệu |
| Sensory | Cấn/Móp, Đẹp, Khô | Cảm quan |
| CatchMethod | Handline, Long line, PS, PS Muối | Phương thức đánh bắt |
| FreezeMethod | Landfrozen, Seafrozen | Cấp đông |
| Certificate | EUCC, COA, MSC | Giấy tờ kèm theo |
| Origin | Việt Nam, Nhập khẩu | Nguồn gốc |
| Market | USA, EU, OTHER | Thị trường XK |
| Warehouse / Carrier / ContainerType | catalog vận hành | Kho, hãng tàu, 20RF/40RF |

## Luật chứng từ thị trường (MarketCertificateRule)

- USA: Must have COA, Add-on MSC (chỉ khi khách cần).
- Châu Âu: Must have EUCC, Add-on MSC.
- Khác: không đòi hỏi.

## Nhóm SKU thành phẩm

Cây SKU trên sheet dùng cho **tỷ lệ thu hồi** khi đưa vào sản xuất:

- **Tuna loin Co (USA)**: 2-4 AAA USA SAKU, 1-2 AAA, LOIN AAA CUT SAKU, 2-4 AA USA, 4UP AA USA, CUT STEAK, 5UPA SAKU, LOIN AA CUT CUBE + tail Co (cube/steak/ground).
- **Tuna loin Co A**: 5UPA, 3-5A, 2-3A — thị trường USA/khác.
- **Tuna loin Vitamin**: 5up, 5-8kg, 2-5kg, 1-2KG, cut saku/steak, tail vitamin — Châu Âu.
- **Tuna loin Co nội địa (NĐ)**: 5UPA NĐ, 3-5A NĐ, 2-3A NĐ, 1-2A NĐ, LOIN CUT STEAK NĐ.
- **NOCO loin / tail**: cut cube, 2-5KG, 5-9LBS, 3-5LBS tail withkin, tail vụn 4G — tất cả thị trường.
- **Rẻo lock 5kg**: NOCO / CO / Vitamin.

Mỗi SKU có `DefaultUnitPriceUsd`, `ExportMarket`, `IsByproduct` (rẻo, vụn, đuôi).

## Page

`/master-data/catalog` — CRUD lookup theo category (Select, không nhập số category).
`/master-data/products` — group + SKU, giá USD.
`/master-data/customers` — Buyer / Supplier / Both, mã, purchaser.
`/master-data/payment-terms` — TT/LC, `RatiosJson` ví dụ `[0.2,0.8]`, `DueDays`.

Xóa master = soft inactive nếu đã phát sinh chứng từ; xóa cứng khi chưa dùng.
