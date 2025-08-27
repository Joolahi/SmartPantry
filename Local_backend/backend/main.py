from fastapi import FastAPI, Depends
from services.auth_services import register_user
from services.login_services import login_user
from schemas import RegisterRequest, LoginRequest, BarcodeData
from services.barcode_service import get_barcode_data, post_barcode_data
from database import SessionLocal, init_db
from sqlalchemy.orm import Session


app = FastAPI()

def get_db():
    db = SessionLocal()
    try:
        yield db
    finally:
        db.close()

@app.on_event("startup")
def on_startup():
    init_db()

@app.get("/")
def read_root():
    return {"Hello": "World"}

@app.post("/register")
def register(req: RegisterRequest):
    return register_user(req)

@app.post("/login")
def login(req: LoginRequest):
    return login_user(req)

@app.get("/barcode/{barcode}")
def get_barcode(barcode : str):
    return get_barcode_data(barcode)

@app.post("/barcode")
def create_barcode_entry(data : BarcodeData, db: Session = Depends(get_db)):
    return post_barcode_data(db, data)