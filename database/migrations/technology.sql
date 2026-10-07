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

START TRANSACTION;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM technology.__ef_migrations_history WHERE "MigrationId" = '20260904091808_BidangDanKematanganKonten') THEN
    CREATE TABLE technology.fields (
        "Id" uuid NOT NULL,
        "Slug" character varying(120) NOT NULL,
        "Name" character varying(120) NOT NULL,
        "Summary" character varying(1000) NOT NULL,
        "Priority" character varying(20) NOT NULL,
        "DisplayOrder" integer NOT NULL,
        CONSTRAINT "PK_fields" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM technology.__ef_migrations_history WHERE "MigrationId" = '20260904091808_BidangDanKematanganKonten') THEN
    INSERT INTO technology.fields ("Id", "DisplayOrder", "Name", "Priority", "Slug", "Summary")
    VALUES ('f1e10000-0000-7000-8000-000000000001', 1, 'AI & Machine Learning', 'Core', 'ai-machine-learning', 'Fondasi: ML klasik, deep learning, LLM, computer vision, NLP, reinforcement learning, multimodal.');
    INSERT INTO technology.fields ("Id", "DisplayOrder", "Name", "Priority", "Slug", "Summary")
    VALUES ('f1e10000-0000-7000-8000-000000000002', 2, 'AI Agents', 'Core', 'ai-agents', 'Lapisan orkestrasi: tool use, MCP, A2A, memori agen, evals, keamanan agen, human-in-the-loop.');
    INSERT INTO technology.fields ("Id", "DisplayOrder", "Name", "Priority", "Slug", "Summary")
    VALUES ('f1e10000-0000-7000-8000-000000000003', 3, 'Cybersecurity', 'Core', 'cybersecurity', 'Ethical hacking, SOC, malware, reverse engineering, keamanan awan, dan keamanan AI.');
    INSERT INTO technology.fields ("Id", "DisplayOrder", "Name", "Priority", "Slug", "Summary")
    VALUES ('f1e10000-0000-7000-8000-000000000004', 4, 'Cloud & Infrastructure', 'Core', 'cloud-infrastructure', 'Docker, Kubernetes, tiga hyperscaler, DevOps, dan platform engineering. Nama lebar dipertahankan dengan sengaja - lihat ADR-010.');
    INSERT INTO technology.fields ("Id", "DisplayOrder", "Name", "Priority", "Slug", "Summary")
    VALUES ('f1e10000-0000-7000-8000-000000000005', 5, 'Data Engineering', 'Core', 'data-engineering', 'Lapisan yang menentukan proyek AI hidup atau mati: ingestion, orkestrasi, lakehouse, format tabel terbuka, streaming, kontrak data, pemodelan.');
    INSERT INTO technology.fields ("Id", "DisplayOrder", "Name", "Priority", "Slug", "Summary")
    VALUES ('f1e10000-0000-7000-8000-000000000006', 6, 'IoT', 'Core', 'iot', 'Konektivitas, firmware & RTOS, Matter/Thread, gateway tepi, keamanan perangkat, IIoT, telemetri deret waktu, dan satu halaman jembatan TinyML.');
    INSERT INTO technology.fields ("Id", "DisplayOrder", "Name", "Priority", "Slug", "Summary")
    VALUES ('f1e10000-0000-7000-8000-000000000007', 7, 'Edge AI', 'Supporting', 'edge-ai', 'AI yang berjalan di perangkat: kuantisasi, distilasi, NPU, runtime on-device. Bidang sendiri, BUKAN anak IoT - tinyML Foundation sendiri sudah berganti nama jadi Edge AI Foundation.');
    INSERT INTO technology.fields ("Id", "DisplayOrder", "Name", "Priority", "Slug", "Summary")
    VALUES ('f1e10000-0000-7000-8000-000000000008', 8, 'Robotics', 'Supporting', 'robotics', 'ROS2, humanoid, drone, kendaraan otonom, dan Robotics AI yang pindah ke sini dari AI & ML.');
    INSERT INTO technology.fields ("Id", "DisplayOrder", "Name", "Priority", "Slug", "Summary")
    VALUES ('f1e10000-0000-7000-8000-000000000009', 9, 'Quantum Computing', 'Supporting', 'quantum-computing', 'Qubit, Qiskit, algoritma kuantum, kriptografi kuantum. Era qubit logis; keunggulan komersial belum ada.');
    INSERT INTO technology.fields ("Id", "DisplayOrder", "Name", "Priority", "Slug", "Summary")
    VALUES ('f1e10000-0000-7000-8000-00000000000a', 10, 'Biotechnology', 'Supporting', 'biotechnology', 'CRISPR, AlphaFold, biologi sintetis, kesehatan digital.');
    INSERT INTO technology.fields ("Id", "DisplayOrder", "Name", "Priority", "Slug", "Summary")
    VALUES ('f1e10000-0000-7000-8000-00000000000b', 11, 'Blockchain', 'Supporting', 'blockchain', 'Smart contract, Ethereum, Solana, Layer 2, DeFi.');
    INSERT INTO technology.fields ("Id", "DisplayOrder", "Name", "Priority", "Slug", "Summary")
    VALUES ('f1e10000-0000-7000-8000-00000000000c', 12, 'Renewable Energy', 'Supporting', 'renewable-energy', 'Solar PV, angin, panas bumi, hidro, bioenergi & SAF, hidrogen hijau, integrasi jaringan & penyimpanan, ekonomi & kebijakan. Fusi TIDAK di sini - lihat ADR-010.');
    INSERT INTO technology.fields ("Id", "DisplayOrder", "Name", "Priority", "Slug", "Summary")
    VALUES ('f1e10000-0000-7000-8000-00000000000d', 13, 'Space Technology', 'Peripheral', 'space-technology', 'Konektivitas LEO, akses ke orbit, smallsat, segmen darat, observasi Bumi, GNSS/PNT, keselamatan orbit. Irisan terkuatnya: 3GPP NTN.');
    INSERT INTO technology.fields ("Id", "DisplayOrder", "Name", "Priority", "Slug", "Summary")
    VALUES ('f1e10000-0000-7000-8000-00000000000e', 14, 'XR (AR/VR/MR)', 'Peripheral', 'xr', 'Realitas diperluas. Dipertahankan sebagai pintu pencarian, tapi sengaja tidak diinvestasikan - lihat ADR-010.');
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM technology.__ef_migrations_history WHERE "MigrationId" = '20260904091808_BidangDanKematanganKonten') THEN
    CREATE UNIQUE INDEX ix_fields_display_order ON technology.fields ("DisplayOrder");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM technology.__ef_migrations_history WHERE "MigrationId" = '20260904091808_BidangDanKematanganKonten') THEN
    CREATE UNIQUE INDEX ix_fields_name ON technology.fields ("Name");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM technology.__ef_migrations_history WHERE "MigrationId" = '20260904091808_BidangDanKematanganKonten') THEN
    CREATE UNIQUE INDEX ix_fields_slug ON technology.fields ("Slug");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM technology.__ef_migrations_history WHERE "MigrationId" = '20260904091808_BidangDanKematanganKonten') THEN
    ALTER TABLE technology.technologies ADD "FieldId" uuid;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM technology.__ef_migrations_history WHERE "MigrationId" = '20260904091808_BidangDanKematanganKonten') THEN
    ALTER TABLE technology.technologies ADD "Maturity" character varying(20);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM technology.__ef_migrations_history WHERE "MigrationId" = '20260904091808_BidangDanKematanganKonten') THEN
    ALTER TABLE technology.technologies ADD "ReviewedAt" timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM technology.__ef_migrations_history WHERE "MigrationId" = '20260904091808_BidangDanKematanganKonten') THEN
    ALTER TABLE technology.technologies ADD "ReviewedBy" character varying(120);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM technology.__ef_migrations_history WHERE "MigrationId" = '20260904091808_BidangDanKematanganKonten') THEN

                    UPDATE technology.technologies AS t
                    SET "FieldId" = f."Id"
                    FROM technology.fields AS f
                    WHERE t."Category" = f."Name";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM technology.__ef_migrations_history WHERE "MigrationId" = '20260904091808_BidangDanKematanganKonten') THEN

                    UPDATE technology.technologies
                    SET "Maturity" = 'Curated'
                    WHERE "Maturity" IS NULL;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM technology.__ef_migrations_history WHERE "MigrationId" = '20260904091808_BidangDanKematanganKonten') THEN

                    DO $$
                    DECLARE tersisa text;
                    BEGIN
                        SELECT string_agg(DISTINCT "Category", ', ')
                        INTO tersisa
                        FROM technology.technologies
                        WHERE "FieldId" IS NULL;

                        IF tersisa IS NOT NULL THEN
                            RAISE EXCEPTION
                                'Migrasi berhenti: kategori lama berikut bukan salah satu dari 14 bidang ADR-010: %. Perbaiki kategorinya dulu, atau kalau ini basis data pengembangan berisi contoh saja, jalankan `run.ps1 reset` lalu migrate ulang.',
                                tersisa;
                        END IF;
                    END $$;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM technology.__ef_migrations_history WHERE "MigrationId" = '20260904091808_BidangDanKematanganKonten') THEN
    ALTER TABLE technology.technologies ALTER COLUMN "Maturity" TYPE character varying(20);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM technology.__ef_migrations_history WHERE "MigrationId" = '20260904091808_BidangDanKematanganKonten') THEN
    DROP INDEX technology.ix_technologies_category;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM technology.__ef_migrations_history WHERE "MigrationId" = '20260904091808_BidangDanKematanganKonten') THEN
    ALTER TABLE technology.technologies DROP COLUMN "Category";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM technology.__ef_migrations_history WHERE "MigrationId" = '20260904091808_BidangDanKematanganKonten') THEN
    CREATE INDEX ix_technologies_field_id ON technology.technologies ("FieldId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM technology.__ef_migrations_history WHERE "MigrationId" = '20260904091808_BidangDanKematanganKonten') THEN
    CREATE INDEX ix_technologies_field_maturity ON technology.technologies ("FieldId", "Maturity");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM technology.__ef_migrations_history WHERE "MigrationId" = '20260904091808_BidangDanKematanganKonten') THEN
    ALTER TABLE technology.technologies ADD CONSTRAINT "FK_technologies_fields_FieldId" FOREIGN KEY ("FieldId") REFERENCES technology.fields ("Id") ON DELETE RESTRICT;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM technology.__ef_migrations_history WHERE "MigrationId" = '20260904091808_BidangDanKematanganKonten') THEN
    INSERT INTO technology.__ef_migrations_history ("MigrationId", "ProductVersion")
    VALUES ('20260904091808_BidangDanKematanganKonten', '10.0.4');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM technology.__ef_migrations_history WHERE "MigrationId" = '20260908063009_EmpatBagianIsiHalaman') THEN
    CREATE TABLE technology.projects (
        "Id" uuid NOT NULL,
        "TechnologyId" uuid NOT NULL,
        "Title" character varying(200) NOT NULL,
        "Brief" character varying(4000) NOT NULL,
        "CreatedAt" timestamp with time zone NOT NULL,
        CONSTRAINT "PK_projects" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_projects_technologies_TechnologyId" FOREIGN KEY ("TechnologyId") REFERENCES technology.technologies ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM technology.__ef_migrations_history WHERE "MigrationId" = '20260908063009_EmpatBagianIsiHalaman') THEN
    CREATE TABLE technology.resources (
        "Id" uuid NOT NULL,
        "TechnologyId" uuid NOT NULL,
        "Type" character varying(20) NOT NULL,
        "Title" character varying(300) NOT NULL,
        "Url" character varying(1000) NOT NULL,
        "CreatedAt" timestamp with time zone NOT NULL,
        CONSTRAINT "PK_resources" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_resources_technologies_TechnologyId" FOREIGN KEY ("TechnologyId") REFERENCES technology.technologies ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM technology.__ef_migrations_history WHERE "MigrationId" = '20260908063009_EmpatBagianIsiHalaman') THEN
    CREATE TABLE technology.roadmap_steps (
        "Id" uuid NOT NULL,
        "TechnologyId" uuid NOT NULL,
        "Order" integer NOT NULL,
        "Title" character varying(200) NOT NULL,
        "Description" character varying(2000) NOT NULL,
        "CreatedAt" timestamp with time zone NOT NULL,
        CONSTRAINT "PK_roadmap_steps" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_roadmap_steps_technologies_TechnologyId" FOREIGN KEY ("TechnologyId") REFERENCES technology.technologies ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM technology.__ef_migrations_history WHERE "MigrationId" = '20260908063009_EmpatBagianIsiHalaman') THEN
    CREATE TABLE technology.tools (
        "Id" uuid NOT NULL,
        "Slug" character varying(160) NOT NULL,
        "Name" character varying(200) NOT NULL,
        "Summary" character varying(2000) NOT NULL,
        "Homepage" character varying(500),
        "CreatedAt" timestamp with time zone NOT NULL,
        CONSTRAINT "PK_tools" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM technology.__ef_migrations_history WHERE "MigrationId" = '20260908063009_EmpatBagianIsiHalaman') THEN
    CREATE TABLE technology.technology_tools (
        "TechnologyId" uuid NOT NULL,
        "ToolId" uuid NOT NULL,
        "Note" character varying(500),
        "CreatedAt" timestamp with time zone NOT NULL,
        CONSTRAINT "PK_technology_tools" PRIMARY KEY ("TechnologyId", "ToolId"),
        CONSTRAINT "FK_technology_tools_technologies_TechnologyId" FOREIGN KEY ("TechnologyId") REFERENCES technology.technologies ("Id") ON DELETE CASCADE,
        CONSTRAINT "FK_technology_tools_tools_ToolId" FOREIGN KEY ("ToolId") REFERENCES technology.tools ("Id") ON DELETE RESTRICT
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM technology.__ef_migrations_history WHERE "MigrationId" = '20260908063009_EmpatBagianIsiHalaman') THEN
    CREATE INDEX "IX_projects_TechnologyId" ON technology.projects ("TechnologyId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM technology.__ef_migrations_history WHERE "MigrationId" = '20260908063009_EmpatBagianIsiHalaman') THEN
    CREATE INDEX ix_resources_technology_type ON technology.resources ("TechnologyId", "Type");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM technology.__ef_migrations_history WHERE "MigrationId" = '20260908063009_EmpatBagianIsiHalaman') THEN
    CREATE UNIQUE INDEX ix_roadmap_steps_technology_order ON technology.roadmap_steps ("TechnologyId", "Order");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM technology.__ef_migrations_history WHERE "MigrationId" = '20260908063009_EmpatBagianIsiHalaman') THEN
    CREATE INDEX ix_technology_tools_tool_id ON technology.technology_tools ("ToolId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM technology.__ef_migrations_history WHERE "MigrationId" = '20260908063009_EmpatBagianIsiHalaman') THEN
    CREATE UNIQUE INDEX ix_tools_slug ON technology.tools ("Slug");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM technology.__ef_migrations_history WHERE "MigrationId" = '20260908063009_EmpatBagianIsiHalaman') THEN
    INSERT INTO technology.__ef_migrations_history ("MigrationId", "ProductVersion")
    VALUES ('20260908063009_EmpatBagianIsiHalaman', '10.0.4');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM technology.__ef_migrations_history WHERE "MigrationId" = '20260911075051_PencarianTeksPenuh') THEN
    ALTER TABLE technology.technologies ADD search_vector tsvector GENERATED ALWAYS AS (setweight(to_tsvector('english', coalesce("Name", '')), 'A') || setweight(to_tsvector('english', coalesce("Summary", '')), 'B')) STORED;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM technology.__ef_migrations_history WHERE "MigrationId" = '20260911075051_PencarianTeksPenuh') THEN
    ALTER TABLE technology.fields ADD search_vector tsvector GENERATED ALWAYS AS (setweight(to_tsvector('english', coalesce("Name", '')), 'A') || setweight(to_tsvector('english', coalesce("Summary", '')), 'B')) STORED;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM technology.__ef_migrations_history WHERE "MigrationId" = '20260911075051_PencarianTeksPenuh') THEN
    CREATE INDEX ix_technologies_search_vector ON technology.technologies USING GIN (search_vector);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM technology.__ef_migrations_history WHERE "MigrationId" = '20260911075051_PencarianTeksPenuh') THEN
    CREATE INDEX ix_fields_search_vector ON technology.fields USING GIN (search_vector);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM technology.__ef_migrations_history WHERE "MigrationId" = '20260911075051_PencarianTeksPenuh') THEN
    INSERT INTO technology.__ef_migrations_history ("MigrationId", "ProductVersion")
    VALUES ('20260911075051_PencarianTeksPenuh', '10.0.4');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM technology.__ef_migrations_history WHERE "MigrationId" = '20260916103308_RelasiAntarTopik') THEN
    CREATE INDEX ix_technology_relationships_to_technology_id ON technology.technology_relationships ("ToTechnologyId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM technology.__ef_migrations_history WHERE "MigrationId" = '20260916103308_RelasiAntarTopik') THEN
    ALTER TABLE technology.technology_relationships ADD CONSTRAINT ck_technology_relationships_bukan_diri_sendiri CHECK ("FromTechnologyId" <> "ToTechnologyId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM technology.__ef_migrations_history WHERE "MigrationId" = '20260916103308_RelasiAntarTopik') THEN
    ALTER TABLE technology.technology_relationships ADD CONSTRAINT ck_technology_relationships_kind CHECK ("Kind" IN ('Requires'));
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM technology.__ef_migrations_history WHERE "MigrationId" = '20260916103308_RelasiAntarTopik') THEN
    ALTER TABLE technology.technology_relationships ADD CONSTRAINT "FK_technology_relationships_technologies_ToTechnologyId" FOREIGN KEY ("ToTechnologyId") REFERENCES technology.technologies ("Id") ON DELETE RESTRICT;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM technology.__ef_migrations_history WHERE "MigrationId" = '20260916103308_RelasiAntarTopik') THEN
    INSERT INTO technology.__ef_migrations_history ("MigrationId", "ProductVersion")
    VALUES ('20260916103308_RelasiAntarTopik', '10.0.4');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM technology.__ef_migrations_history WHERE "MigrationId" = '20260917024242_RingkasanBidangTanpaRujukanADR') THEN
    UPDATE technology.fields SET "Summary" = 'Docker, Kubernetes, tiga hyperscaler, DevOps, dan platform engineering.'
    WHERE "Id" = 'f1e10000-0000-7000-8000-000000000004';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM technology.__ef_migrations_history WHERE "MigrationId" = '20260917024242_RingkasanBidangTanpaRujukanADR') THEN
    UPDATE technology.fields SET "Summary" = 'Solar PV, angin, panas bumi, hidro, bioenergi & SAF, hidrogen hijau, integrasi jaringan & penyimpanan, ekonomi & kebijakan. Fusi nuklir tidak termasuk.'
    WHERE "Id" = 'f1e10000-0000-7000-8000-00000000000c';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM technology.__ef_migrations_history WHERE "MigrationId" = '20260917024242_RingkasanBidangTanpaRujukanADR') THEN
    UPDATE technology.fields SET "Summary" = 'Realitas diperluas. Dipertahankan sebagai pintu pencarian, tapi sengaja tidak diinvestasikan.'
    WHERE "Id" = 'f1e10000-0000-7000-8000-00000000000e';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM technology.__ef_migrations_history WHERE "MigrationId" = '20260917024242_RingkasanBidangTanpaRujukanADR') THEN
    INSERT INTO technology.__ef_migrations_history ("MigrationId", "ProductVersion")
    VALUES ('20260917024242_RingkasanBidangTanpaRujukanADR', '10.0.4');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM technology.__ef_migrations_history WHERE "MigrationId" = '20261007045020_TigaBidangDeepTech') THEN
    INSERT INTO technology.fields ("Id", "DisplayOrder", "Name", "Priority", "Slug", "Summary")
    VALUES ('f1e10000-0000-7000-8000-00000000000f', 15, 'Advanced Computing & Hardware', 'Supporting', 'advanced-computing-hardware', 'Arsitektur mikro dan komputasi masa depan: desain semikonduktor, RISC-V, neuromorphic computing, fotonika, dan DNA data storage.');
    INSERT INTO technology.fields ("Id", "DisplayOrder", "Name", "Priority", "Slug", "Summary")
    VALUES ('f1e10000-0000-7000-8000-000000000010', 16, 'Neurotechnology & BCI', 'Peripheral', 'neurotechnology-bci', 'Antarmuka langsung manusia-mesin: Brain-Computer Interface (BCI) invasif dan non-invasif, neuroprostetika, EEG signal processing, dan implan saraf.');
    INSERT INTO technology.fields ("Id", "DisplayOrder", "Name", "Priority", "Slug", "Summary")
    VALUES ('f1e10000-0000-7000-8000-000000000011', 17, 'Advanced Materials & Nanotech', 'Peripheral', 'advanced-materials-nanotech', 'Landasan fisik deep tech: grafena, metamaterial, superkonduktor (suhu kamar belum terbukti), dan rekayasa material skala nano untuk baterai dan antariksa.');
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM technology.__ef_migrations_history WHERE "MigrationId" = '20261007045020_TigaBidangDeepTech') THEN
    INSERT INTO technology.__ef_migrations_history ("MigrationId", "ProductVersion")
    VALUES ('20261007045020_TigaBidangDeepTech', '10.0.4');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM technology.__ef_migrations_history WHERE "MigrationId" = '20261007050935_MediaDanOverview') THEN
    ALTER TABLE technology.technologies ADD "Overview" character varying(20000);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM technology.__ef_migrations_history WHERE "MigrationId" = '20261007050935_MediaDanOverview') THEN
    CREATE TABLE technology.technology_media (
        "Id" uuid NOT NULL,
        "TechnologyId" uuid NOT NULL,
        "Key" character varying(80) NOT NULL,
        "Kind" character varying(10) NOT NULL,
        "Url" character varying(300),
        "VideoId" character varying(11),
        "Alt" character varying(500) NOT NULL,
        "Caption" character varying(1000),
        "SourceName" character varying(200),
        "SourceUrl" character varying(1000),
        "License" character varying(200) NOT NULL,
        "CreatedAt" timestamp with time zone NOT NULL,
        CONSTRAINT "PK_technology_media" PRIMARY KEY ("Id"),
        CONSTRAINT ck_technology_media_alt CHECK (btrim("Alt") <> ''),
        CONSTRAINT ck_technology_media_bentuk CHECK (("Kind" = 'Image' AND "Url" IS NOT NULL AND "VideoId" IS NULL) OR ("Kind" = 'Video' AND "VideoId" IS NOT NULL AND "Url" IS NULL)),
        CONSTRAINT ck_technology_media_berkas_sendiri CHECK ("Url" IS NULL OR "Url" LIKE '/media/%'),
        CONSTRAINT ck_technology_media_kind CHECK ("Kind" IN ('Image', 'Video')),
        CONSTRAINT ck_technology_media_kunci CHECK ("Key" ~ '^[a-z0-9]+(-[a-z0-9]+)*$'),
        CONSTRAINT ck_technology_media_lisensi CHECK (btrim("License") <> '' AND ("License" = 'Karya sendiri' OR ("SourceName" IS NOT NULL AND "SourceUrl" IS NOT NULL))),
        CONSTRAINT "FK_technology_media_technologies_TechnologyId" FOREIGN KEY ("TechnologyId") REFERENCES technology.technologies ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM technology.__ef_migrations_history WHERE "MigrationId" = '20261007050935_MediaDanOverview') THEN
    CREATE UNIQUE INDEX ix_technology_media_technology_key ON technology.technology_media ("TechnologyId", "Key");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM technology.__ef_migrations_history WHERE "MigrationId" = '20261007050935_MediaDanOverview') THEN
    INSERT INTO technology.__ef_migrations_history ("MigrationId", "ProductVersion")
    VALUES ('20261007050935_MediaDanOverview', '10.0.4');
    END IF;
END $EF$;
COMMIT;

