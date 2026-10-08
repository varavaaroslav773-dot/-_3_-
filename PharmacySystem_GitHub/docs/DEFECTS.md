# Журнал дефектов

DEF-01 — отсутствие запрета продажи сверх остатка. Severity: High, Priority: High. Исправление: проверка `medicine.Quantity < quantity` в `MedicineService.Sell`.

DEF-02 — возможность провести продажу нулевого количества. Severity: Medium, Priority: High. Исправление: проверка `quantity <= 0`.

DEF-03 — возможность сохранить отрицательный остаток при добавлении/изменении товара. Severity: High, Priority: High. Исправление: проверка `quantity < 0` в общей валидации.
