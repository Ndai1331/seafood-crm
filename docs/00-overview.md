# Tổng quan Seafood CRM

CRM nhà máy chế biến cá ngừ: theo dõi nguyên liệu nhập khẩu, sản xuất loin/tail/rẻo, chứng từ thị trường (COA/EUCC/MSC), hợp đồng xuất khẩu, thanh toán và deposit.

## Đơn vị & tiền tệ

- Khối lượng nội bộ: **kg**. Đơn hàng xuất khẩu nhập được **lb hoặc kg**, hệ thống quy đổi 2 chiều.
- Giá bán / invoice / deposit / AR: **USD**.
- Giá mua nguyên liệu trên sheet sản xuất có thể theo **VND/kg** kèm tỷ giá (vd 26.300). P&L lot tính cả USD và VND.

## Trạng thái hợp đồng (`ContractStatus`)

Draft → PendingApproval → Signed → ReadyStock → ReadyDocs → Shipped → Paid. Cancelled = 9.

## Matching chứng từ

- USA: must-have **COA**, addon **MSC**.
- Châu Âu: must-have **EUCC**, addon **MSC**.
- Khác / nội địa: không bắt buộc chứng từ XK.
- MSC có thể kèm EUCC hoặc COA để đi nhiều thị trường.

## UI conventions

- CRUD master & nghiệp vụ: **modal** (Large / ExtraLarge), không form-above-table.
- Nút Primary = Tạo/Lưu; Secondary = Hủy/Tải lại/Gợi ý; Success = Duyệt; Danger = Xóa.
- i18n: `L["code"]` đọc `wwwroot/locales/vi.json` và `en.json`.
- Tiền: `UsdMoneyInput` (`1,234.56`) và `VnMoneyInput` (`1.234.567 ₫`).
