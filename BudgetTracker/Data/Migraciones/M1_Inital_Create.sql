-- ============================================
-- TABLE: public.users
-- ============================================

CREATE TABLE IF NOT EXISTS public.users
(
    user_id INTEGER GENERATED ALWAYS AS IDENTITY,
    user_name VARCHAR(250) NOT NULL,
    user_email VARCHAR(250) NOT NULL,
    user_password VARCHAR(250),

    CONSTRAINT users_pkey PRIMARY KEY (user_id),
    CONSTRAINT users_user_email_key UNIQUE (user_email),
    CONSTRAINT users_user_name_key UNIQUE (user_name)
);

-- ============================================
-- TABLE: public.categorys
-- ============================================

CREATE TABLE IF NOT EXISTS public.categorys
(
    category_id INTEGER GENERATED ALWAYS AS IDENTITY,
    category_name VARCHAR(250) NOT NULL,

    CONSTRAINT categorys_pkey PRIMARY KEY (category_id)
);

-- ============================================
-- TABLE: public.bills
-- ============================================

CREATE TABLE IF NOT EXISTS public.bills
(
    bills_id INTEGER GENERATED ALWAYS AS IDENTITY,
    bills_date DATE NOT NULL,
    bills_amount NUMERIC(10,0) NOT NULL,
    bills_desc VARCHAR(250) DEFAULT NULL,
    category_id INTEGER NOT NULL,
    user_id INTEGER NOT NULL,

    CONSTRAINT bills_pkey PRIMARY KEY (bills_id),

    CONSTRAINT bills_category_id_fkey 
        FOREIGN KEY (category_id)
        REFERENCES public.categorys (category_id)
        ON UPDATE NO ACTION
        ON DELETE NO ACTION,

    CONSTRAINT bills_user_id_fkey 
        FOREIGN KEY (user_id)
        REFERENCES public.users (user_id)
        ON UPDATE NO ACTION
        ON DELETE NO ACTION
);

-- ============================================
-- TABLE: public.incomes
-- ============================================

CREATE TABLE IF NOT EXISTS public.incomes
(
    income_id INTEGER GENERATED ALWAYS AS IDENTITY,
    income_date DATE NOT NULL,
    income_amount NUMERIC(10,0) NOT NULL,
    income_desc VARCHAR(250) DEFAULT NULL,
    user_id INTEGER NOT NULL,
    category_id INTEGER NOT NULL,

    CONSTRAINT incomes_pkey PRIMARY KEY (income_id),

    CONSTRAINT incomes_category_id_fkey 
        FOREIGN KEY (category_id)
        REFERENCES public.categorys (category_id)
        ON UPDATE NO ACTION
        ON DELETE NO ACTION,

    CONSTRAINT incomes_user_id_fkey 
        FOREIGN KEY (user_id)
        REFERENCES public.users (user_id)
        ON UPDATE NO ACTION
        ON DELETE NO ACTION
);

-- ============================================
-- TABLE: public.monthly_totals
-- ============================================

CREATE TABLE IF NOT EXISTS public.monthly_totals
(
    monthly_totals_id INTEGER GENERATED ALWAYS AS IDENTITY,
    monthly_totals_year INTEGER NOT NULL,
    monthly_totals_month INTEGER NOT NULL,
    total_income NUMERIC(10,0),
    total_bill NUMERIC(10,0),
    user_id INTEGER NOT NULL,

    CONSTRAINT monthly_totals_pkey PRIMARY KEY (monthly_totals_id),

    CONSTRAINT monthly_totals_user_id_fkey 
        FOREIGN KEY (user_id)
        REFERENCES public.users (user_id)
        ON UPDATE NO ACTION
        ON DELETE NO ACTION
);