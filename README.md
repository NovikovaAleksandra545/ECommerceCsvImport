# ECommerceCsvImport

Модуль импорта товаров в интернет-магазин из CSV-файла с хранением в памяти.
Производственная практика ПП.02.01, вариант 3 «Одежда».

Выполнила: Новикова Александра

## Технологии

- C#, ASP.NET Core MVC (.NET 8)
- xUnit для тестов
- Bootstrap (из шаблона проекта) + свои стили

## Как запустить

Открыть `ECommerceCsvImport.sln` в Visual Studio и запустить проект `ECommerceCsvImport` (F5).

Или из консоли:

```
dotnet run --project src/ECommerceCsvImport
dotnet test
```

Дальше: «Загрузка из CSV» → путь `/csv/sample_products.csv` → «Импортировать».
Должно получиться: всего строк 11, импортировано 11, ошибок 0.

## Формат CSV

```
sku,name,description,price,currency,category_name,stock,image_url,is_active,size,color,material
3001,Футболка белая,Летняя футболка,990,RUB,Футболки,25,/images/tshirt-white.jpg,true,M,белый,хлопок
```

| Поле | Проверка |
|------|----------|
| sku | не пустое, без повторов |
| name | не пустое |
| price | число больше 0 |
| currency | если пусто — RUB |
| category_name | если пусто — «Без категории» |
| stock | целое число, не меньше 0 |
| is_active | true / false / 1 / 0 |
| size | S, M, L, XL или число от 30 до 70 |
| color | не пустое |
| description, image_url, material | могут быть пустыми |

Если артикул уже есть (в файле или в памяти), строка пропускается и выводится ошибка.
Файл может быть в UTF-8 или Windows-1251, разделитель — запятая или точка с запятой.

## Структура

```
src/ECommerceCsvImport
  Controllers/ProductsController.cs   - страницы каталога и импорта
  Models/                             - Product, ImportResult, ImportError, модели страниц
  Services/InMemoryProductRepository  - хранение товаров в List (singleton)
  Services/ProductImportService       - чтение CSV и проверка строк
  Services/CsvLineParser              - разбор строки CSV с кавычками
  Views/Products                      - Index (каталог), Import, Details
  wwwroot/csv                         - примеры CSV для проверки
tests/ECommerceCsvImport.Tests        - тесты репозитория, парсера и сервиса импорта
docs/screenshots                      - скриншоты страниц
```

## Что сделано дополнительно

- загрузка файла через браузер
- поиск, фильтр по категории и размеру, сортировка
- страница товара
- общий остаток и стоимость остатков
- выгрузка каталога в CSV
- кодировка Windows-1251 и разделитель «;»

## Тестовые сценарии

| № | Файл | Ожидаемый результат | Результат |
|---|------|---------------------|-----------|
| 1 | sample_products.csv | 11 строк, 11 импортировано | ок |
| 2 | invalid_price.csv | ошибка цены в строке 2 | ок |
| 3 | empty_file.csv | «Данные для импорта отсутствуют» | ок |
| 4 | duplicate_sku.csv | 1 импортирован, 1 дубликат | ок |
| 5 | negative_stock.csv | ошибка остатка | ок |
| 6 | cyrillic_windows1251.csv | кириллица читается правильно | ок |
| 7 | missing_color.csv | ошибка «color обязательно» | ок |
| 8 | invalid_size.csv | ошибка размера | ок |
| 9 | mixed_errors.csv | 10 строк, 8 импортировано, 2 ошибки | ок |
