# Інструкція для виправлення бази даних

## Проблема
База даних не містить колонок `IsNew` та `IsBestSeller` в таблиці `Products`.

## Рішення 1: Автоматичне (рекомендовано)
Просто перезапустіть додаток - код автоматично додасть колонки при старті.

## Рішення 2: Вручну через SQL Server Management Studio

1. Відкрийте SQL Server Management Studio
2. Підключіться до вашої бази даних: `(localdb)\mssqllocaldb`
3. Виберіть базу даних: `FoodDeliveryDb`
4. Виконайте наступний SQL скрипт:

```sql
-- Додати колонку IsNew
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Products]') AND name = 'IsNew')
BEGIN
    ALTER TABLE [dbo].[Products] ADD [IsNew] bit NOT NULL DEFAULT 0;
    PRINT 'Column IsNew added successfully';
END
GO

-- Додати колонку IsBestSeller
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Products]') AND name = 'IsBestSeller')
BEGIN
    ALTER TABLE [dbo].[Products] ADD [IsBestSeller] bit NOT NULL DEFAULT 0;
    PRINT 'Column IsBestSeller added successfully';
END
GO
```

## Рішення 3: Через Visual Studio

1. Відкрийте **View** → **SQL Server Object Explorer**
2. Розгорніть `(localdb)\MSSQLLocalDB` → **Databases** → **FoodDeliveryDb** → **Tables** → **Products**
3. Клацніть правою кнопкою на **Products** → **View Designer**
4. Додайте колонки:
   - `IsNew` (bit, NOT NULL, Default: 0)
   - `IsBestSeller` (bit, NOT NULL, Default: 0)
5. Збережіть зміни

## Перевірка
Після виконання будь-якого з рішень, перезапустіть додаток. Помилка має зникнути.

