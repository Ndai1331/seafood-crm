# Thanh toán — sheet «Xuất khẩu - PAYMENT TRACKING»

Theo dõi từng invoice / đợt thanh toán.

## Header theo đơn

Số TBSX, mã+tên khách, ETD, ETA, số invoice, tổng tiền USD, phương thức TT.

## Chi tiết đợt (PaymentInstallment)

Sinh từ `PaymentTerm.RatiosJson`:

- TT (20+80) → 2 đợt ratio 0.2 và 0.8
- TT (20+60+20) → 3 đợt
- LC 45 DAYS (PASS FDA) → 1 đợt, DueDate = ETD/ETA + DueDays

Mỗi đợt: tỷ lệ, số tiền, ngày nhận, hạn thanh toán.

TT = thanh toán trực tiếp. LC = bảo lãnh ngân hàng.

## Page `/finance/payments`

Bảng đơn + expand đợt. Modal «Nhận tiền» với `UsdMoneyInput`. Không nhập Id thô.
