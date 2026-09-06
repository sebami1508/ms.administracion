-- Cambios requeridos para: restablecimiento de contraseña con OTP
-- Ejecutar en la base de datos PostgreSQL ANTES de publicar los servicios.

-- Tabla de OTP para restablecimiento de contraseña (usuarios existentes).
-- Referencia a TA_USUARIO por USUARIO_ID.
CREATE TABLE IF NOT EXISTS "SC_ADMINISTRACION"."TA_OTP" (
    "OTP_ID"           uuid PRIMARY KEY,
    "USUARIO_ID"       varchar(40)  NOT NULL,
    "PROPOSITO"        varchar(30)  NOT NULL,
    "OTP_HASH"         varchar(128) NOT NULL,
    "OTP_SALT"         varchar(64)  NOT NULL,
    "FECHA_CREACION"   timestamp without time zone NOT NULL,
    "FECHA_EXPIRACION" timestamp without time zone NOT NULL,
    "USADO"            boolean NOT NULL DEFAULT false,
    "FECHA_USO"        timestamp without time zone NULL,
    "INTENTOS"         integer NOT NULL DEFAULT 0,
    "MAX_INTENTOS"     integer NOT NULL DEFAULT 5,
    "IP_SOLICITUD"     varchar(45)  NULL,
    "USER_AGENT"       varchar(250) NULL,
    CONSTRAINT "FK_TA_OTP_USUARIO"
        FOREIGN KEY ("USUARIO_ID")
        REFERENCES "SC_ADMINISTRACION"."TA_USUARIO" ("USUARIO_ID")
);

CREATE INDEX IF NOT EXISTS "IX_TA_OTP_USUARIO"
    ON "SC_ADMINISTRACION"."TA_OTP" ("USUARIO_ID", "PROPOSITO", "USADO");
