create table created_only_entities (
  id uuid not null,
  content text not null,
  created_at timestamptz not null,
  constraint pk_created_only_entities primary key (id)
);
