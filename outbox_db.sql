create sequence payment_outbox_id_seq as integer;

create table payment_outbox
(
    id int8 default nextval('payment_outbox_id_seq'::regclass) not null,
    entity_id uuid not null,
    retry_count int4 not null
);

create unique index payment_outbox_id_uindex
    on payment_outbox (id);

alter table payment_outbox
    add constraint payment_outbox_pk
        primary key (id);