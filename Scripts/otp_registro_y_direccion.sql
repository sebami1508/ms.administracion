-- Cambios requeridos para: OTP de registro + dirección del usuario
-- Ejecutar en la base de datos PostgreSQL ANTES de publicar los servicios.

-- 1. Dirección en los datos del usuario
ALTER TABLE "SC_ADMINISTRACION"."TA_USUARIO"
    ADD COLUMN IF NOT EXISTS "DIRECCION" varchar(250) NULL;

-- 2. Tabla de OTP para verificación de correo en el registro de clientes
CREATE TABLE IF NOT EXISTS "SC_ADMINISTRACION"."TA_OTP_REGISTRO" (
    "OTP_REGISTRO_ID"  uuid PRIMARY KEY,
    "CORREO"           varchar(150) NOT NULL,
    "OTP_HASH"         varchar(128) NOT NULL,
    "OTP_SALT"         varchar(64)  NOT NULL,
    "FECHA_CREACION"   timestamp without time zone NOT NULL,
    "FECHA_EXPIRACION" timestamp without time zone NOT NULL,
    "USADO"            boolean NOT NULL DEFAULT false,
    "FECHA_USO"        timestamp without time zone NULL,
    "INTENTOS"         integer NOT NULL DEFAULT 0,
    "MAX_INTENTOS"     integer NOT NULL DEFAULT 5
);

CREATE INDEX IF NOT EXISTS "IX_TA_OTP_REGISTRO_CORREO"
    ON "SC_ADMINISTRACION"."TA_OTP_REGISTRO" ("CORREO", "USADO");
