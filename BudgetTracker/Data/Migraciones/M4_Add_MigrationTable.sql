-- ============================================
-- TABLE: public.migrations
-- ============================================

CREATE TABLE IF NOT EXISTS public.migrations
(
    migration_name VARCHAR(250) NOT NULL,

    CONSTRAINT migrations_pkey PRIMARY KEY (migration_name)
);

-- ============================================
-- Registro de migraciones ejecutadas
-- ============================================

INSERT INTO public.migrations (migration_name) VALUES
('M1_Inital_Create.sql'),
('M2_Identity_Table.sql'),
('M3_Add_SubCategorys.sql'),
('M4_Add_MigrationTable.sql');
