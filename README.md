# Домашнее задание №7

### Проблема
Ваша Кафка нестабильна, иногда запись в неё заканчивается ошибкой.

Но вам необходимо обеспечить 100% запись ваших событий в Кафку.

### Решение
Чтобы обеспечить транзакционность записи, необходимо реализовать на проекте паттерн [TransactionalOutbox](https://microservices.io/patterns/data/transactional-outbox.html)

Для этого:
* Создаёте в БД отдельную таблицу payment_outbox.
* В коде, который раньше слушал доменные события и отправлял сообщения в Кафку, делаете запись в эту таблицу.
* Создаёте отдельный BackgroundWorker, который опрашивает эту таблицу на наличие новых сообщений (период опроса 1 секунда, можно вынести в конфиг).
* Каждое сообщение он пытается записать в Кафку.
* Если успех, то удаляет сообщение из таблицы.
* Если неудача, то оставляет сообщение в таблице, но увеличивает значение количества попыток отправки.
* Пытается делать отправку, пока не превысит лимит (10 раз).
