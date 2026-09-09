# Nhập nguyên liệu — sheet «Theo dõi hàng nhập hằng ngày»

Bảng theo dõi NL nhập khẩu về nhà máy. Một phiếu nhập (header) có nhiều dòng commodity (size/form khác nhau cùng cont).

## Header (InboundPurchase)

- Khách hàng, mã khách, số HĐ nhập, Purchaser
- Thiếu chứng từ (CTU)
- ETA / hàng đến cảng, ngày về kho, tên kho (`WarehouseId`)
- CARE = thị trường đích (USA / EU / Khác)
- Payment term, ngày thanh toán dự kiến
- BL, Cont, loại cont (1x40 / 20RF / 40RF)
- Xe vận chuyển cảng → nhà máy
- Số lô nguyên liệu, số tờ khai hải quan, xuất xứ
- Actual weight, lượng thực nhận / viết lại (dư/thiếu kg)
- Ngày lưu cảng = ngày kéo cont − ngày hàng đến
- Chi phí hàng nhập (VND hoặc USD tuỳ ghi nhận): phí lấy lệnh, điện lạnh, nâng hạ, hải quan, hạ tầng
- Ghi chú, cờ đã nhập file NL

## Dòng hàng (InboundLine)

- Commodity (vd TUNA WR 30 KGS UP)
- kg theo phương thức: Long line, Handline, PS, Land
- Mahi (kg), thành phẩm (lbs)
- Quant. CONT, Price USD/kg, Invoice amount USD

Invoice amount = tổng kg × price (cho phép override).

## Page `/inbound`

Modal ExtraLarge, tab Header | Dòng hàng | Chi phí. Select khách / kho / PTTT / thị trường. `UsdMoneyInput` cho giá và invoice. Tự tính ngày lưu cảng khi có ETA và ngày kéo cont.
