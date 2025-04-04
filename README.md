# FuturesPriceComparison
# Микросервис для регулярной проверки цены активов

Для запуска:
`docker compose up postgres -d` (создает БД Postgres)  
Затем (первый запуск постгреса долгий)
`docker compose up -d` (Создает приложение, Prometheus, Grafana)  


Grafana dashboards: http://localhost:3000/dashboards  
Login: `Admin`  
Password: `Admin`

---

# Описание работы

Сервис опрашивает `binance api`, получает актуальную цену для пары активов, сохранияет в базу разницу цен и полученные цены.  
При отсутсвии возможности получить информация из `api` используется последнее значение из базы.  
При отсутствии данных и в базе имформация об этом логируется как ошибка.  

Разница хранится в коллекции `price_difference`, цены хранятся в коллекции `futures_prices`.  

Выбор фьючерсов для парсинга возможен добавлением интересующих фьючерсов в коллекцию `futures` и их `id` в `futures_pairs_to_check`  

Инервал опроса настраивается в файле `appsettings.json`.

``` json
  "ScheduleOptions" : {
    "Interval" : "01:00:00"
  }
```
