CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory" (
    "MigrationId" character varying(150) NOT NULL,
    "ProductVersion" character varying(32) NOT NULL,
    CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY ("MigrationId")
);

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20250917021433_AddKickstarterTables') THEN
    CREATE TABLE "Categories" (
        "Id" bigint NOT NULL,
        "Name" text NOT NULL,
        "Slug" text NOT NULL,
        "ParentId" bigint,
        "ParentName" text NOT NULL,
        CONSTRAINT "PK_Categories" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20250917021433_AddKickstarterTables') THEN
    CREATE TABLE "Creators" (
        "Id" bigint NOT NULL,
        "Name" text NOT NULL,
        CONSTRAINT "PK_Creators" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20250917021433_AddKickstarterTables') THEN
    CREATE TABLE "Locations" (
        "Id" bigint NOT NULL,
        "Name" text NOT NULL,
        "DisplayableName" text NOT NULL,
        "Country" text NOT NULL,
        "State" text NOT NULL,
        "Type" text NOT NULL,
        CONSTRAINT "PK_Locations" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20250917021433_AddKickstarterTables') THEN
    CREATE TABLE "KickstarterProjects" (
        "Id" bigint NOT NULL,
        "Name" text NOT NULL,
        "Blurb" text NOT NULL,
        "Goal" numeric NOT NULL,
        "Pledged" numeric NOT NULL,
        "State" text NOT NULL,
        "Country" text NOT NULL,
        "Currency" text NOT NULL,
        "Deadline" timestamp with time zone NOT NULL,
        "CreatedAt" timestamp with time zone NOT NULL,
        "LaunchedAt" timestamp with time zone NOT NULL,
        "BackersCount" integer NOT NULL,
        "UsdPledged" numeric NOT NULL,
        "CreatorId" bigint NOT NULL,
        "CategoryId" bigint NOT NULL,
        "LocationId" bigint NOT NULL,
        "Photo" jsonb NOT NULL,
        "Urls" jsonb NOT NULL,
        CONSTRAINT "PK_KickstarterProjects" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_KickstarterProjects_Categories_CategoryId" FOREIGN KEY ("CategoryId") REFERENCES "Categories" ("Id") ON DELETE CASCADE,
        CONSTRAINT "FK_KickstarterProjects_Creators_CreatorId" FOREIGN KEY ("CreatorId") REFERENCES "Creators" ("Id") ON DELETE CASCADE,
        CONSTRAINT "FK_KickstarterProjects_Locations_LocationId" FOREIGN KEY ("LocationId") REFERENCES "Locations" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20250917021433_AddKickstarterTables') THEN
    CREATE INDEX "IX_KickstarterProjects_CategoryId" ON "KickstarterProjects" ("CategoryId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20250917021433_AddKickstarterTables') THEN
    CREATE INDEX "IX_KickstarterProjects_CreatorId" ON "KickstarterProjects" ("CreatorId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20250917021433_AddKickstarterTables') THEN
    CREATE INDEX "IX_KickstarterProjects_LocationId" ON "KickstarterProjects" ("LocationId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20250917021433_AddKickstarterTables') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20250917021433_AddKickstarterTables', '8.0.4');
    END IF;
END $EF$;
COMMIT;

