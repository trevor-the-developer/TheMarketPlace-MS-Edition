-- Connect to the listings database
\connect listings

-- Create Categories table
CREATE TABLE IF NOT EXISTS "Categories" (
    "Id" UUID PRIMARY KEY,
    "Name" VARCHAR(100) NOT NULL,
    "Description" VARCHAR(500),
    "ParentCategoryId" UUID,
    "IsActive" BOOLEAN NOT NULL DEFAULT TRUE,
    "CreatedAt" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "UpdatedAt" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY ("ParentCategoryId") REFERENCES "Categories"("Id")
);

-- Create index on Name for faster lookup
CREATE INDEX IF NOT EXISTS "IX_Categories_Name" ON "Categories"("Name");

-- Create index on ParentCategoryId for faster lookup of hierarchies
CREATE INDEX IF NOT EXISTS "IX_Categories_ParentCategoryId" ON "Categories"("ParentCategoryId");

-- Create the __EFMigrationsHistory table for Entity Framework
CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory" (
    "MigrationId" VARCHAR(150) NOT NULL,
    "ProductVersion" VARCHAR(32) NOT NULL,
    PRIMARY KEY ("MigrationId")
);

-- Grant all privileges on all tables in listings database to postgres user
GRANT ALL PRIVILEGES ON ALL TABLES IN SCHEMA public TO postgres;
GRANT ALL PRIVILEGES ON ALL SEQUENCES IN SCHEMA public TO postgres;

