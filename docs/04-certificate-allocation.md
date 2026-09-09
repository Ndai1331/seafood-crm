# Sắp xếp giấy / allocation — sheet «Ví dụ sắp xếp giấy»

Đây là logic kho **nội bộ** để ghép lô nguyên liệu + thành phẩm với đơn hàng xuất, chỉnh định mức để làm tròn kg NL.

## Bốn khối dữ liệu

1. **Hàng sản xuất theo thực tế**: lot, ngày nhập, SKU thu hồi, kg TP, kg NL dùng, định mức, tỷ lệ thu hồi, thị trường XK, giấy tờ lô.
2. **Đơn hàng cần xuất**: số HĐ, SKU, số lượng, thị trường, tình trạng chứng từ (đủ CT / đủ hàng thiếu CT / thiếu NL).
3. **SL trong kho**: on-hand SKU, nhu cầu HĐ mở, SL còn lại.
4. **Trù lùi NL đã sử dụng (nội bộ)**: phân bổ lot → HĐ, chỉnh định mức sau, check OK / Thiếu NL / Dư NL.

## Rule matching

- Thị trường USA chỉ lấy lot có **COA** (MSC addon nếu khách yêu cầu).
- EU chỉ lấy lot **EUCC**.
- Không lấy lot giấy tờ lệch thị trường.
- Phụ phẩm (vụn, đuôi, rẻo nội địa) không cần CT XK.
- Khi thiếu NL cho một HĐ: lấy bổ sung từ lô khác, **chỉnh định mức** để kg NL sử dụng làm tròn về 0 hoặc số nguyên dễ khai báo.
- Khi dư NL: có thể tăng định mức nhẹ để giảm số giấy tờ khai.

## Trạng thái chứng từ đơn

- Đủ chứng từ
- Đủ hàng trong kho nhưng chưa có chứng từ → cần mua thêm NL đúng giấy
- Thiếu NL

## Page `/inventory/allocate`

Board: cột HĐ mở, thẻ lot khả dụng (SKU + kg + certs). Gán lot → line HĐ, nhập kg và yield after adjust, ghi chú nội bộ. API lưu `LotAllocation`.

`/inventory` vẫn là bảng tồn + warning.
