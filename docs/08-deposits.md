# Deposit — sheet «Xuất khẩu _ DEPOSIT»

Khách trả trước, phân bổ vào nhiều HĐ.

## Deposit

- Khách, số tiền USD, ngày nhận, ghi chú

## Allocation

- Số HĐ, số tiền allocate, tình trạng ĐÃ XUẤT + ngày xuất
- Một deposit có thể cover nhiều HĐ (vd Khách 110: 132.830 USD chia 6 HĐ)

Số dư remaining = AmountUsd − sum(allocations).

Khi xuất hàng xong, đánh dấu allocation `IsExported` + `ExportedAt`.

## Page `/finance/deposits`

Modal 2 bước: nhận deposit | allocate vào HĐ (Select HĐ của cùng khách). Hiện remaining trên bảng.
