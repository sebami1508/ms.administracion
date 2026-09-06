-- ============================================================
-- Tabla para almacenar los tokens FCM de los dispositivos
-- de los clientes (notificaciones push de la App).
-- Ejecutar en la base de datos de ms.administracion.
-- ============================================================

CREATE TABLE IF NOT EXISTS "SC_ADMINISTRACION"."TA_DISPOSITIVO_FCM"
(
    "DISPOSITIVO_ID"  uuid          NOT NULL,
    "USUARIO_ID"      varchar(40)   NOT NULL,
    "TOKEN"           varchar(300)  NOT NULL,
    "PLATAFORMA"      varchar(20)   NULL,
    "FECHA_REGISTRO"  timestamp without time zone NOT NULL,
    CONSTRAINT "PK_TA_DISPOSITIVO_FCM" PRIMARY KEY ("DISPOSITIVO_ID")
);

-- Un token es único por dispositivo (evita duplicados / permite upsert)
CREATE UNIQUE INDEX IF NOT EXISTS "IX_TA_DISPOSITIVO_FCM_TOKEN"
    ON "SC_ADMINISTRACION"."TA_DISPOSITIVO_FCM" ("TOKEN");

-- Índice para buscar rápidamente por usuario al enviar la notificación
CREATE INDEX IF NOT EXISTS "IX_TA_DISPOSITIVO_FCM_USUARIO"
    ON "SC_ADMINISTRACION"."TA_DISPOSITIVO_FCM" ("USUARIO_ID");
