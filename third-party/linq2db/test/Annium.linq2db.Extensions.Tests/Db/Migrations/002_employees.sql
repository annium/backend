CREATE TABLE employees (
  id uuid NOT NULL,
  company_id uuid NOT NULL,
  name text NOT NULL,
  CONSTRAINT pk_employees PRIMARY KEY (id),
  CONSTRAINT fk_employees_companies_company_id FOREIGN KEY (company_id) REFERENCES companies(id) ON DELETE RESTRICT
);
