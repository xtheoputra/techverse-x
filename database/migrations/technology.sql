DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM pg_namespace WHERE nspname = 'technology') THEN
        CREATE SCHEMA technology;
    END IF;
END $EF$;
CREATE TABLE IF NOT EXISTS technology.__ef_migrations_history (
    "MigrationId" character varying(150) NOT NULL,
    "ProductVersion" character varying(32) NOT NULL,
    CONSTRAINT "PK___ef_migrations_history" PRIMARY KEY ("MigrationId")
);

START TRANSACTION;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM technology.__ef_migrations_history WHERE "MigrationId" = '20260903023635_InitialTechnologySchema') THEN
        IF NOT EXISTS(SELECT 1 FROM pg_namespace WHERE nspname = 'technology') THEN
            CREATE SCHEMA technology;
        END IF;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM technology.__ef_migrations_history WHERE "MigrationId" = '20260903023635_InitialTechnologySchema') THEN
    CREATE TABLE technology.technologies (
        "Id" uuid NOT NULL,
        "Slug" character varying(160) NOT NULL,
        "Name" character varying(200) NOT NULL,
        "Summary" character varying(2000) NOT NULL,
        "Category" character varying(120) NOT NULL,
        "Status" character varying(20) NOT NULL,
        "CreatedAt" timestamp with time zone NOT NULL,
        "UpdatedAt" timestamp with time zone NOT NULL,
        CONSTRAINT "PK_technologies" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM technology.__ef_migrations_history WHERE "MigrationId" = '20260903023635_InitialTechnologySchema') THEN
    CREATE TABLE technology.technology_relationships (
        "Id" uuid NOT NULL,
        "FromTechnologyId" uuid NOT NULL,
        "ToTechnologyId" uuid NOT NULL,
        "Kind" character varying(20) NOT NULL,
        "CreatedAt" timestamp with time zone NOT NULL,
        CONSTRAINT "PK_technology_relationships" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_technology_relationships_technologies_FromTechnologyId" FOREIGN KEY ("FromTechnologyId") REFERENCES technology.technologies ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM technology.__ef_migrations_history WHERE "MigrationId" = '20260903023635_InitialTechnologySchema') THEN
    CREATE INDEX ix_technologies_category ON technology.technologies ("Category");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM technology.__ef_migrations_history WHERE "MigrationId" = '20260903023635_InitialTechnologySchema') THEN
    CREATE UNIQUE INDEX ix_technologies_slug ON technology.technologies ("Slug");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM technology.__ef_migrations_history WHERE "MigrationId" = '20260903023635_InitialTechnologySchema') THEN
    CREATE UNIQUE INDEX ix_technology_relationships_edge ON technology.technology_relationships ("FromTechnologyId", "ToTechnologyId", "Kind");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM technology.__ef_migrations_history WHERE "MigrationId" = '20260903023635_InitialTechnologySchema') THEN
    INSERT INTO technology.__ef_migrations_history ("MigrationId", "ProductVersion")
    VALUES ('20260903023635_InitialTechnologySchema', '10.0.4');
    END IF;
END $EF$;
COMMIT;

