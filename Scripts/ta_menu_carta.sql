-- ============================================================
-- Menú de la carta en PDF (administrable desde Angular y visible
-- en la App de clientes). Un único PDF vigente a la vez.
-- Ejecutar en la base de datos de ms.administracion.
-- ============================================================

CREATE TABLE IF NOT EXISTS "SC_ADMINISTRACION"."TA_MENU_CARTA"
(
    "MENU_CARTA_ID"   varchar(40)   NOT NULL,
    "NOMBRE_ARCHIVO"  varchar(200)  NOT NULL,
    "CONTENIDO"       text          NOT NULL,   -- PDF en Base64
    "FECHA_REGISTRO"  timestamp without time zone NOT NULL,
    "VIGENTE"         boolean       NOT NULL DEFAULT true,
    CONSTRAINT "PK_TA_MENU_CARTA" PRIMARY KEY ("MENU_CARTA_ID")
);

-- Índice para recuperar rápido el PDF vigente
CREATE INDEX IF NOT EXISTS "IX_TA_MENU_CARTA_VIGENTE"
    ON "SC_ADMINISTRACION"."TA_MENU_CARTA" ("VIGENTE");


-- ============================================================
-- Registro de la opción en el menú lateral de Angular
-- (Administración). Se asigna a Super Administrador y Administrador.
-- ============================================================

DO $$
DECLARE
    v_menu_id     varchar(40) := 'a1c0ffee-cafe-4c0d-9a1e-11e5ca11ab1e';
    v_rol_super   varchar(40) := 'b815aa4b-3e9c-44a9-a40b-05c033d01411';
    v_rol_admin   varchar(40) := 'e08c70ed-94b5-4692-9b47-e6f8a9a23f1f';
BEGIN
    -- Menú (solo si no existe la ruta)
    IF NOT EXISTS (
        SELECT 1 FROM "SC_ADMINISTRACION"."TA_MENU"
        WHERE "RUTA" = '/modulos/menu-carta'
    ) THEN
        INSERT INTO "SC_ADMINISTRACION"."TA_MENU"
            ("MENU_ID", "NOMBRE", "ICONO", "RUTA", "SUB_MENU", "MENU_PADRE", "ORDEN", "VIGENTE")
        VALUES
            (v_menu_id, 'Menú (Carta)', 'fa-solid fa-book-open', '/modulos/menu-carta', false, NULL, 99, true);
    ELSE
        SELECT "MENU_ID" INTO v_menu_id
        FROM "SC_ADMINISTRACION"."TA_MENU"
        WHERE "RUTA" = '/modulos/menu-carta'
        LIMIT 1;
    END IF;

    -- Perfil Super Administrador
    IF NOT EXISTS (
        SELECT 1 FROM "SC_ADMINISTRACION"."TA_PERFILES"
        WHERE "MENU_ID" = v_menu_id AND "ROL_ID" = v_rol_super
    ) THEN
        INSERT INTO "SC_ADMINISTRACION"."TA_PERFILES" ("PERFIL_ID", "MENU_ID", "ROL_ID")
        VALUES (gen_random_uuid()::text, v_menu_id, v_rol_super);
    END IF;

    -- Perfil Administrador
    IF NOT EXISTS (
        SELECT 1 FROM "SC_ADMINISTRACION"."TA_PERFILES"
        WHERE "MENU_ID" = v_menu_id AND "ROL_ID" = v_rol_admin
    ) THEN
        INSERT INTO "SC_ADMINISTRACION"."TA_PERFILES" ("PERFIL_ID", "MENU_ID", "ROL_ID")
        VALUES (gen_random_uuid()::text, v_menu_id, v_rol_admin);
    END IF;
END $$;
