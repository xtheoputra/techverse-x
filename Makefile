# Perintah sehari-hari TechVerse X.
# `make` tanpa argumen menampilkan daftarnya.

SHELL := /bin/sh

TECHNOLOGY_PROJECT := services/technology/TechVerseX.TechnologyService.csproj
API_PROJECT        := apps/api/TechVerseX.Api.csproj

.DEFAULT_GOAL := help

.PHONY: help
help: ## Menampilkan daftar perintah
	@grep -E '^[a-zA-Z_-]+:.*?## .*$$' $(MAKEFILE_LIST) \
		| awk 'BEGIN {FS = ":.*?## "}; {printf "  \033[36m%-14s\033[0m %s\n", $$1, $$2}'

.PHONY: up
up: ## Menyalakan Postgres + Redis
	docker compose up -d
	@echo "Menunggu sampai sehat..."
	@docker compose ps

.PHONY: down
down: ## Mematikan infrastruktur lokal
	docker compose down

.PHONY: reset
reset: ## Mematikan DAN menghapus datanya (tidak bisa dibatalkan)
	docker compose down -v

.PHONY: migrate
migrate: ## Menjalankan migrasi basis data
	dotnet ef database update --project $(TECHNOLOGY_PROJECT) --startup-project $(API_PROJECT)

.PHONY: migration
migration: ## Membuat migrasi baru: make migration NAME=NamaMigrasi
	@test -n "$(NAME)" || (echo "Pakai: make migration NAME=NamaMigrasi" && exit 1)
	dotnet ef migrations add $(NAME) --project $(TECHNOLOGY_PROJECT) --startup-project $(API_PROJECT) --output-dir Infrastructure/Persistence/Migrations

.PHONY: db-script
db-script: ## Menulis SQL migrasi idempoten ke database/migrations/
	dotnet ef migrations script --idempotent --project $(TECHNOLOGY_PROJECT) --startup-project $(API_PROJECT) --output database/migrations/technology.sql

.PHONY: seed
seed: ## Mengisi contoh isi lewat API (API harus sudah jalan)
	@sh database/seeds/seed.sh

.PHONY: api
api: ## Menjalankan API di http://localhost:5080
	dotnet run --project $(API_PROJECT)

.PHONY: web
web: ## Menjalankan web di http://localhost:3000
	npm run dev:web

.PHONY: build
build: ## Build seluruh solusi .NET
	dotnet build TechVerseX.slnx

.PHONY: test
test: ## Menjalankan uji .NET
	dotnet test TechVerseX.slnx

.PHONY: verify
verify: ## Gerbang yang sama dengan CI: build ketat + uji + build web
	dotnet build TechVerseX.slnx --configuration Release
	dotnet test TechVerseX.slnx --no-build --configuration Release
	npm run lint:web
	npm run build:web
