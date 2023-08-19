create table companies (
  id uuid not null,
  name text not null,
  metadata text not null,
  created_at timestamptz not null,
  constraint pk_companies primary key (id)
);