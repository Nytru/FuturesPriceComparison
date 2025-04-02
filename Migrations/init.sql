create table if not exists  exchanges
(
    id   serial
        constraint exchanges_pk
            primary key,
    name text not null
);

alter table if exists exchanges
    owner to postgres;

create table if not exists  futures
(
    id          serial
        constraint futures_pk
            primary key,
    exchange_id integer                                    not null
        constraint futures_exchanges_id_fk
            references exchanges,
    name        varchar(100)                               not null,
    symbol      varchar(100) default ''::character varying not null,
    pair        varchar(100) default ''::character varying not null
);

alter table if exists futures
    owner to postgres;

create table if not exists  futures_prices
(
    price         numeric                  not null,
    timestamp_utc timestamp with time zone not null,
    futures_id    integer                  not null
        constraint futures_prices_futures_id_fk
            references futures,
    id            serial
        constraint futures_prices_pk
            primary key
);

alter table if exists futures_prices
    owner to postgres;

create table if not exists  price_difference
(
    id             serial
        constraint price_difference_pk
            primary key,
    first_futures  integer                  not null
        constraint price_difference_futures_id_fk
            references futures,
    second_futures integer                  not null
        constraint price_difference_futures_id_fk_2
            references futures,
    difference     numeric                  not null,
    timestamp      timestamp with time zone not null
);

alter table if exists price_difference
    owner to postgres;

create table if not exists futures_pairs_to_check
(
    first_futures  integer not null
        constraint pairs_to_check_futures_id_fk
            references futures,
    second_futures integer not null
        constraint pairs_to_check_futures_id_fk_2
            references futures,
    constraint pairs_to_check_pk
        primary key (first_futures, second_futures)
);

alter table if exists futures_pairs_to_check
    owner to postgres;
