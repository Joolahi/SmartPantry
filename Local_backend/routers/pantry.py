from fastapi import APIRouter, Depends
from sqlalchemy.orm import Session
from datetime import date
import uuid
import models, database

router = APIRouter(prefix="/pantry", tags=["Pantry"])


@router.post("/seed")
def seed_pantry_items(db: Session = Depends(database.get_db)):
    items = [
        models.PantryItem(
            user_id=uuid.uuid4(),
            product_name="Milk",
            quantity=2,
            expiry_date=date(2025, 8, 20),
        ),
        models.PantryItem(
            user_id=uuid.uuid4(),
            product_name="Bread",
            quantity=1,
            expiry_date=date(2025, 8, 15),
        ),
    ]
    db.add_all(items)
    db.commit()
    return {"message": "Test data inserted"}


@router.get("/all")
def get_pantry_items(db: Session = Depends(database.get_db)):
    return db.query(models.PantryItem).all()
