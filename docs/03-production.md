# Đưa vào sản xuất — sheet «đưa vào sản xuất»

Một **lô (lot)** gắn phiếu nhập. Ví dụ: nhập 31/07/2026, mã lô `100313092122G`, tỷ giá 26.300.

## Nguyên liệu thực nhận

Phân theo form/size WR (20UP, 20UP-MÓP, 10-20KG) + tổng kg + cont. Có cột điều chỉnh nếu hàng có vấn đề (trừ kg).

## Chỉ số thu hồi

- Lượng BTP thu được (kg)
- Định mức BTP = NL / BTP
- Tổng thu hồi cuối cùng
- Rẻo thu được, định mức thu hồi rẻo
- Giá mua (link từ inbound, VND/kg)
- Giá sau điều chỉnh
- Giá bình quân/kg
- Giá trị thu được (USD + VND)
- Lãi/lỗ dự kiến (USD + VND)

## Cây thành phẩm

Mỗi SKU ghi số kg, % trên BTP làm đẹp, đơn giá USD, thành tiền. Cộng nhóm:

- TỔNG % CO USA
- TỔNG % CO A
- TỔNG % loin vitamin
- TỔNG % CO NỘI ĐỊA
- TỔNG % LOIN NOCO
- TỔNG % TAIL NOCO
- Rẻo lock 5kg

Yield lô = tổng recovered / raw kg. Yield dòng = recovered / raw used (định mức = raw used / recovered).

## Page `/production`

Modal ExtraLarge: tab Nguyên liệu | Thành phẩm (grid SKU) | P&L. Tự tính yield và lãi/lỗ khi có giá mua + tỷ giá + đơn giá SKU.
