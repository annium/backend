create table company_employees (
  company_id uuid not null,
  employee_id uuid not null,
  role text not null,
  constraint pk_company_employees primary key (company_id, employee_id),
  constraint fk_company_employees_companies_company_id foreign key (company_id) references companies(id) on delete restrict
  constraint fk_company_employees_employees_employee_id foreign key (employee_id) references employees(id) on delete restrict
);