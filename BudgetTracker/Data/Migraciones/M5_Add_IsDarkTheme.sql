-- ============================================
-- COLUMN: users.is_dark_theme
-- ============================================

ALTER TABLE public.users
ADD COLUMN IF NOT EXISTS is_dark_theme BOOLEAN NOT NULL DEFAULT false;

-- ============================================
-- Registro de migración
-- ============================================

INSERT INTO public.migrations (migration_name)
VALUES ('M5_Add_IsDarkTheme.sql')
ON CONFLICT DO NOTHING;
