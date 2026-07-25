create table manual_time_entities (
  id uuid not null,
  content text not null,
  created_at timestamptz not null,
  updated_at timestamptz not null,
  constraint pk_manual_time_entities primary key (id)
);
