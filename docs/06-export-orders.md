# Đơn hàng xuất khẩu — sheet «Xuất khẩu - đơn hàng»

Mỗi dòng = một chuyến đóng hàng / invoice.

## Cột

- Ngày đóng hàng, số TBSX (thông báo sản xuất / lô hàng)
- Mã + tên khách
- Thời gian hạ bãi, ghi chú
- ETD, ETA
- Sales contract (có thể 2 số: HĐ công ty + HĐ khách)
- Invoice no, payment term (TT 30+70, LC 45 days, …)
- Tuna cartons, quantity NW-LBS, NW-KGS (auto convert)
- Amount USD
- Cont no, loại 20RF / 40RF
- Carrier, chuyến (điểm đi − điểm đến), FWD name, nhà xe nội địa

Khi lưu đơn: nếu có payment term thì **sinh installment** theo `RatiosJson`. HĐ gắn đơn chuyển `Shipped`.

## Page `/export/orders`

Modal ExtraLarge, Select HĐ/KH/PTTT/carrier/loại cont. Nhập kg hoặc lbs — ô còn lại tự quy đổi.
