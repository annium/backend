CREATE TABLE companies (
  id uuid NOT NULL,
  name text NOT NULL,
  metadata text NOT NULL,
  CONSTRAINT pk_companies PRIMARY KEY (id)
);
