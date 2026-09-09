import DataService from '../_content/BootstrapBlazor.UniverSheet/data-service.js'

const { Disposable, setDependencies, Injector, ICommandService, CommandType, UniverInstanceType } = UniverCore;
const { ContextMenuGroup, ContextMenuPosition, RibbonStartGroup, ComponentManager, IMenuManagerService, MenuItemType, getMenuHiddenObservable } = UniverUi;

const GetDataOperation = {
    id: 'report.operation.add-table',
    type: CommandType.OPERATION,
    handler: async (accessor) => {
        const dataService = accessor.get(DataService.name);
        const data = await dataService.getDataAsync({
            messageName: "getDataMessage",
            commandName: "getDataCommand"
        });
        if (data) {
            const univerAPI = dataService.getUniverSheet().univerAPI;
            const range = univerAPI.getActiveWorkbook().getActiveSheet().getRange(0, 0, 2, 3)
            const defaultData = [
                [{ v: data.data.key }, { v: null }, { v: null }],
                [{ v: data.data.value }, { v: null }, { v: null }]
            ]
            range.setValues(defaultData);
        }
    }
};

const SetCellValuesOperation = {
    id: 'report.operation.set-cell-values',
    type: CommandType.OPERATION,
    handler: async (accessor) => {
        const dataService = accessor.get(DataService.name);
        const data = await dataService.getDataAsync({
            messageName: "setCellValuesMessage",
            commandName: "SetCellValues"
        });

        console.log('SetCellValues received:', data);

        if (data && data.data) {
            try {
                const univerAPI = dataService.getUniverSheet().univerAPI;
                const workbook = univerAPI.getActiveWorkbook();
                const sheet = workbook.getActiveSheet();

                // Parse data
                const cellData = JSON.parse(data.data);
                console.log('Parsed cell data:', cellData);
                console.log('Data has', cellData.rows, 'rows and', cellData.cols, 'columns');

                if (cellData.data && Array.isArray(cellData.data)) {
                    // Clear existing content first
                    try {
                        const maxRow = sheet.getMaxRows();
                        const maxCol = sheet.getMaxColumns();
                        if (maxRow > 0 && maxCol > 0) {
                            const clearRange = sheet.getRange(0, 0, Math.min(maxRow, 100), Math.min(maxCol, 20));
                            clearRange.clear();
                            console.log('Cleared existing content');
                        }
                    } catch (clearError) {
                        console.warn('Could not clear sheet:', clearError);
                    }

                    // Set cell values row by row
                    for (let rowIndex = 0; rowIndex < cellData.data.length; rowIndex++) {
                        const rowData = cellData.data[rowIndex];
                        if (Array.isArray(rowData)) {
                            try {
                                const range = sheet.getRange(rowIndex, 0, 1, rowData.length);
                                range.setValues([rowData]);

                                if (rowIndex === 0) {
                                    // Bold header row
                                    for (let col = 0; col < rowData.length; col++) {
                                        const headerCell = sheet.getRange(rowIndex, col);
                                        headerCell.setFontBold(true);
                                    }
                                }
                            } catch (rowError) {
                                console.error(`Error setting row ${rowIndex}:`, rowError);
                            }
                        }
                    }

                    console.log('Successfully set all cell values');

                    // Refresh sheet
                    try {
                        if (typeof sheet.refresh === 'function') {
                            sheet.refresh();
                        }
                    } catch (refreshError) {
                        console.warn('Could not refresh sheet:', refreshError);
                    }
                } else {
                    console.error('Invalid data format - missing data array');
                }
            } catch (error) {
                console.error('Error in SetCellValues:', error);
            }
        }
    }
};

const SetWorkbookOperation = {
    id: 'report.operation.set-workbook',
    type: CommandType.OPERATION,
    handler: async (accessor) => {
        const dataService = accessor.get(DataService.name);
        const data = await dataService.getDataAsync({
            messageName: "setWorkbookMessage",
            commandName: "setWorkbookCommand"
        });
        console.log(`Set workbook data:  ${data} `);
        console.log(`Set workbook data.data:   ${data.data}`);

        if (data && data.data) {
            try {
                const univerAPI = dataService.getUniverSheet().univerAPI;
                const workbook = univerAPI.getActiveWorkbook();

                // Parse workbook data từ JSON
                const workbookData = JSON.parse(data.data);

                console.log('Parsed workbook data:', workbookData);
                console.log('Sheets in workbook:', Object.keys(workbookData.sheets || {}));
                console.log('Sheet order:', workbookData.sheetOrder);

                // Get all existing sheets
                const existingSheets = workbook.getSheets();
                console.log('Existing sheets:', existingSheets.map(s => ({ id: s.getSheetId(), name: s.getName() })));

                // Process each sheet in workbook data
                if (workbookData.sheets && workbookData.sheetOrder) {
                    const sheetIds = workbookData.sheetOrder;

                    for (let i = 0; i < sheetIds.length; i++) {
                        const sheetId = sheetIds[i];
                        const sheetData = workbookData.sheets[sheetId];

                        if (!sheetData) {
                            console.warn(`Sheet data not found for ID: ${sheetId}`);
                            continue;
                        }

                        console.log(`Processing sheet ${i}: ${sheetData.name} (ID: ${sheetId})`);

                        let sheet = null;

                        // For first sheet, use existing active sheet if available
                        if (i === 0 && existingSheets.length > 0) {
                            sheet = existingSheets[0];
                            console.log(`Using existing sheet: ${sheet.getName()}`);

                            // Clear all content first
                            try {
                                const maxRow = sheet.getMaxRows();
                                const maxCol = sheet.getMaxColumns();
                                if (maxRow > 0 && maxCol > 0) {
                                    const clearRange = sheet.getRange(0, 0, maxRow, maxCol);
                                    clearRange.clear();
                                    console.log(`Cleared sheet content: ${maxRow}x${maxCol}`);
                                }
                            } catch (clearError) {
                                console.warn('Could not clear sheet, continuing:', clearError);
                            }
                        } else {
                            // For subsequent sheets, check if sheet already exists
                            const existingSheet = existingSheets.find(s => s.getName() === sheetData.name);
                            if (existingSheet) {
                                sheet = existingSheet;
                                console.log(`Found existing sheet with name: ${sheetData.name}`);
                            } else {
                                // Create new sheet
                                try {
                                    // Try using insertSheet API
                                    sheet = workbook.insertSheet(i, sheetData.name);
                                    console.log(`Created new sheet: ${sheetData.name} at position ${i}`);
                                } catch (insertError) {
                                    console.error(`Failed to insert sheet: ${insertError}`);
                                    // Fallback to active sheet
                                    sheet = workbook.getActiveSheet();
                                }
                            }
                        }

                        if (sheet && sheetData.cellData) {
                            console.log(`Populating sheet ${sheetData.name} with data...`);

                            // Cập nhật cell data
                            Object.keys(sheetData.cellData).forEach(rowIndex => {
                                const rowData = sheetData.cellData[rowIndex];
                                Object.keys(rowData).forEach(colIndex => {
                                    const cellData = rowData[colIndex];
                                    if (cellData.v !== undefined) {
                                        try {
                                            const range = sheet.getRange(parseInt(rowIndex), parseInt(colIndex));
                                            range.setValue(cellData.v);

                                            // Cập nhật style nếu có
                                            if (cellData.s && workbookData.styles && workbookData.styles[cellData.s]) {
                                                const style = workbookData.styles[cellData.s];

                                                // Cập nhật background color
                                                if (style.bg && style.bg.rgb) {
                                                    try {
                                                        const bgColor = convertColorToRgb(style.bg.rgb);
                                                        range.setBackgroundColor(bgColor);
                                                    } catch (error) {
                                                        console.error('Error setting background color:', error);
                                                    }
                                                }

                                                // Cập nhật text color
                                                if (style.cl && style.cl.rgb) {
                                                    try {
                                                        const textColor = convertColorToRgb(style.cl.rgb);
                                                        range.setFontColor(textColor);
                                                    } catch (error) {
                                                        console.error('Error setting font color:', error);
                                                    }
                                                }

                                                // Cập nhật bold
                                                if (style.bl !== undefined) {
                                                    try {
                                                        range.setFontBold(style.bl === 1);
                                                    } catch (error) {
                                                        console.error('Error setting bold:', error);
                                                    }
                                                }
                                            }
                                        } catch (cellError) {
                                            console.error(`Error setting cell [${rowIndex},${colIndex}]:`, cellError);
                                        }
                                    }
                                });
                            });

                            console.log(`Finished populating sheet ${sheetData.name}`);
                        }
                    }
                }

                // Đợi một chút để đảm bảo tất cả thay đổi được áp dụng
                await new Promise(resolve => setTimeout(resolve, 100));

                // Force refresh worksheet và trigger render
                const activeSheet = workbook.getActiveSheet();
                if (activeSheet) {
                    // Trigger multiple refresh methods để đảm bảo UI được cập nhật
                    try {
                        if (typeof activeSheet.refresh === 'function') {
                            activeSheet.refresh();
                        }

                        // Force re-render của sheet
                        if (typeof activeSheet.render === 'function') {
                            activeSheet.render();
                        }

                        // Trigger change event để UI biết có thay đổi
                        if (typeof activeSheet.trigger === 'function') {
                            activeSheet.trigger('change');
                        }

                        console.log('Sheet refreshed and re-rendered successfully');
                    } catch (refreshError) {
                        console.warn('Some refresh methods failed, but continuing:', refreshError);
                    }
                }

                console.log('Workbook updated successfully');
            } catch (error) {
                console.error('Error updating workbook:', error);
            }
        }
    }
};

// Helper function để chuyển đổi màu sắc sang định dạng RGB
function convertColorToRgb(color) {
    if (!color) return 'rgb(255,255,255)';
    
    // Nếu đã là định dạng RGB thì giữ nguyên
    if (color.startsWith('rgb(')) {
        return color;
    }
    
    // Nếu là hex color thì chuyển sang RGB
    if (color.startsWith('#')) {
        try {
            const hex = color.substring(1);
            if (hex.length === 6) {
                const r = parseInt(hex.substring(0, 2), 16);
                const g = parseInt(hex.substring(2, 4), 16);
                const b = parseInt(hex.substring(4, 6), 16);
                return `rgb(${r},${g},${b})`;
            }
        } catch (error) {
            console.error('Error converting hex to RGB:', error);
        }
    }
    
    // Nếu là tên màu tiếng Anh thì chuyển sang RGB
    const colorMap = {
        'red': 'rgb(255,0,0)',
        'green': 'rgb(0,128,0)',
        'blue': 'rgb(0,0,255)',
        'yellow': 'rgb(255,255,0)',
        'black': 'rgb(0,0,0)',
        'white': 'rgb(255,255,255)',
        'orange': 'rgb(255,165,0)',
        'purple': 'rgb(128,0,128)',
        'pink': 'rgb(255,192,203)',
        'brown': 'rgb(165,42,42)',
        'gray': 'rgb(128,128,128)',
        'cyan': 'rgb(0,255,255)',
        'magenta': 'rgb(255,0,255)'
    };
    
    if (colorMap[color.toLowerCase()]) {
        return colorMap[color.toLowerCase()];
    }
    
    // Mặc định trả về màu trắng
    console.warn(`Unknown color format: ${color}, using white as default`);
    return 'rgb(255,255,255)';
}

function GetDataIcon() {
    return React.createElement(
        'svg',
        { xmlns: "http://www.w3.org/2000/svg", width: "1em", height: "1em", viewBox: "0 0 24 24" },
        React.createElement(
            'path',
            { fill: "currentColor", d: "M12 2c5.523 0 10 4.477 10 10s-4.477 10-10 10S2 17.523 2 12S6.477 2 12 2m.16 14a6.981 6.981 0 0 0-5.147 2.256A7.966 7.966 0 0 0 12 20a7.97 7.97 0 0 0 5.167-1.892A6.979 6.979 0 0 0 12.16 16M12 4a8 8 0 0 0-6.384 12.821A8.975 8.975 0 0 1 12.16 14a8.972 8.972 0 0 1 6.362 2.634A8 8 0 0 0 12 4m0 1a4 4 0 1 1 0 8a4 4 0 0 1 0-8m0 2a2 2 0 1 0 0 4a2 2 0 0 0 0-4" }
        )
    );
}

function ReportGetDataFactory(accessor) {
    return {
        id: GetDataOperation.id,
        type: MenuItemType.BUTTON,
        icon: 'GetDataIcon',
        tooltip: 'Lưu dữ liệu',
        title: 'Lưu dữ liệu',
        hidden$: getMenuHiddenObservable(accessor, UniverInstanceType.UNIVER_SHEET)
    };
}

export class ReportController extends Disposable {
    constructor(_injector, _commandService, _menuManagerService, _componentManager) {
        super();

        this._injector = _injector;
        this._commandService = _commandService;
        this._menuManagerService = _menuManagerService;
        this._componentManager = _componentManager;

        this._initCommands();
        this._registerComponents();
        this._initMenus();
        this._registerReceiveDataCallback();
    }

    _registerReceiveDataCallback() {
        const dataService = this._injector.get(DataService.name);
        dataService.registerReceiveDataCallback(data => {
            this.receiveData(data, dataService.getUniverSheet());
        });
    }

    _initCommands() {
        [GetDataOperation, SetWorkbookOperation, SetCellValuesOperation].forEach((c) => {
            this.disposeWithMe(this._commandService.registerCommand(c));
        });
    }

    _registerComponents() {
        const componentMap = {
            GetDataIcon,
        }
        Object.entries(componentMap).forEach((item) => {
            this.disposeWithMe(this._componentManager.register(...item));
        });

    }

    _initMenus() {
        this._menuManagerService.mergeMenu({
            [RibbonStartGroup.HISTORY]: {
                [GetDataOperation.id]: {
                    order: -1,
                    menuItemFactory: ReportGetDataFactory
                },
            },
            [ContextMenuPosition.MAIN_AREA]: {
                [ContextMenuGroup.DATA]: {
                    [GetDataOperation.id]: {
                        order: 0,
                        menuItemFactory: ReportGetDataFactory
                    }
                }
            }
        });
    }

    receiveData(data, sheet) {
        const { univerAPI } = sheet;
        const range = univerAPI.getActiveWorkbook().getActiveSheet().getRange(0, 0, 2, 3)
        const defaultData = [
            [{ v: 'Domain' }, { v: 'Team' }, { v: 'PIC' }],
            [{ v: "paste vào đây để thêm mới" }, { v: "" }, { v: "" }]
        ]
        range.setValues(defaultData);
    }
}

setDependencies(ReportController, [Injector, ICommandService, IMenuManagerService, ComponentManager]);
