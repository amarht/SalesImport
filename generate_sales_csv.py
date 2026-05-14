import csv
import random
from datetime import datetime, timedelta

# =========================
# Configuration
# =========================
OUTPUT_FILE = "Sales.csv"

# Taille cible ~50 GB
TARGET_SIZE_GB = 50
TARGET_SIZE_BYTES = TARGET_SIZE_GB * 1024 * 1024 * 1024

# Jeux de données
STORE_CODES = [f"STR{str(i).zfill(4)}" for i in range(1, 501)]
PRODUCT_CODES = [f"PRD{str(i).zfill(6)}" for i in range(1, 10001)]

START_DATE = datetime(2020, 1, 1)
END_DATE = datetime(2026, 12, 31)

# =========================
# Fonctions utilitaires
# =========================
def random_date():
    delta = END_DATE - START_DATE
    random_days = random.randint(0, delta.days)
    random_seconds = random.randint(0, 86399)

    date = START_DATE + timedelta(
        days=random_days,
        seconds=random_seconds
    )

    return date.strftime("%Y-%m-%d %H:%M:%S")


def generate_row(sale_number):
    store_code = random.choice(STORE_CODES)
    product_code = random.choice(PRODUCT_CODES)

    quantity = random.randint(1, 20)

    # Prix entre 1€ et 2000€
    unit_price = round(random.uniform(1.0, 2000.0), 2)

    sale_date = random_date()

    return [
        sale_number,
        store_code,
        product_code,
        quantity,
        unit_price,
        sale_date
    ]


# =========================
# Génération du CSV
# =========================
def generate_csv():
    current_size = 0
    sale_number = 1

    print(f"Génération du fichier {OUTPUT_FILE} (~{TARGET_SIZE_GB} GB)")
    print("Cela peut prendre plusieurs heures selon la machine.")

    with open(OUTPUT_FILE, mode="w", newline="", encoding="utf-8") as file:
        writer = csv.writer(file)

        # Header
        header = [
            "SaleNumber",
            "StoreCode",
            "ProductCode",
            "Quantity",
            "UnitPrice",
            "SaleDate"
        ]

        writer.writerow(header)

        # Flush initial pour mesurer
        file.flush()
        current_size = file.tell()

        while current_size < TARGET_SIZE_BYTES:

            row = generate_row(sale_number)
            writer.writerow(row)

            sale_number += 1

            # Vérification périodique de la taille
            if sale_number % 100000 == 0:
                file.flush()
                current_size = file.tell()

                gb_written = current_size / (1024 ** 3)

                print(
                    f"Lignes: {sale_number:,} | "
                    f"Taille: {gb_written:.2f} GB"
                )

    print("Terminé.")
    print(f"Nombre total de lignes: {sale_number:,}")


if __name__ == "__main__":
    generate_csv()
