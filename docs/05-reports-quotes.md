# Báo cáo & báo giá — sheet «Các task chính cần có»

## Báo cáo

- Danh sách hàng về / hàng xuất: filter mọi cột người dùng muốn; **favorite** các báo cáo thường dùng.
- Dashboard chính: **ghim** báo cáo, thể hiện **pie chart** (tồn theo SKU, xuất theo tuyến).
- Quy đổi pound ↔ kg trên đơn XK.
- SL và giá trị hàng đã mua / đã bán.
- Hiệu quả từng đơn hàng (P&L = doanh thu USD − giá vốn lot phân bổ).

## Báo giá / hợp đồng (kiểu hóa đơn điện tử)

1. Chọn khách hàng + thị trường.
2. Thêm dòng: SKU, khối lượng kg hoặc lbs, đơn giá USD (gợi ý từ HĐ gần nhất cùng KH+SKU).
3. Hệ thống sinh số HĐ nếu để trống.
4. So sánh giá vs HĐ cũ (cùng khách) và vs giá bán khách khác **2 tháng gần nhất**.
5. Duyệt HĐ → Signed. Phân tích biến động hiện trên modal duyệt.

## Page

- `/dashboard` — KPI + pie + pin.
- `/reports` — inbound / export / order P&L + favorite.
- `/export/quotes` — modal nhiều dòng, suggest, approve trên hàng.
