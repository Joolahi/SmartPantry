import requests
from fastapi import HTTPException
from sqlalchemy.orm import session
from models import BarcodeEntry
from schemas import BarcodeData

def get_barcode_data(barcode: str):
    try:
        url = f"https://world.openfoodfacts.org/api/v0/product/{barcode}.json"
        response = requests.get(url, timeout=10)

        if response.status_code != 200:
            raise HTTPException(status_code=response.status_code, detail="Error fetching data from Open Food Facts")
        
        data = response.json()

        if data.get("status") != 1:
            raise HTTPException(status_code=404, detail="Product not found")
        
        product = data.get("product", {})

        return {
            "barcode": product.get("code"),
            "name": product.get("product_name", ""),
            "brand": ", ".join(product.get("brands_tags", [])) if product.get("brands_tags") else "Tuntematon",
            "energy" : product.get("nutriments", {}).get("energy-kcal_100g", 0.0),
            "image" : product.get("image_thumb_url", ""),
        }
    except requests.RequestException as e:
        raise HTTPException(status_code=500, detail=f"Request failed: {str(e)}")

def post_barcode_data(db: session, data: BarcodeData):
    new_entry = BarcodeEntry(
            barcode=data.barcode,
            name=data.name,
            brand=data.brand,
            energy=int(data.energy),
            image=data.image or "",
            best_before=data.bestBefore,
            user_id=data.userId
        )
    db.add(new_entry)
    db.commit()
    db.refresh(new_entry)

    return {"message": "Barcode data saved successfully", "entry_id": new_entry.id}