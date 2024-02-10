create table employees (
  id uuid not null,
  chief_id uuid null,
  name text not null,
  created_at timestamptz not null,
  updated_at timestamptz not null,
  constraint pk_employees primary key (id),
  constraint fk_employees_employees_chief_id foreign key (chief_id) references employees(id) on delete restrict
);