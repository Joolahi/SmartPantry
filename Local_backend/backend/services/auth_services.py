from passlib.context import CryptContext
from database import SessionLocal
from models import User
from fastapi import HTTPException

pwd_context = CryptContext(schemes=["sha256_crypt"], deprecated="auto")

def register_user(req):
    with SessionLocal() as db:
        exisiting_user = db.query(User).filter(User.email == req.email).first()
        if exisiting_user:
            raise HTTPException(status_code=400, detail="Email already registered")
        
        hashed_password = pwd_context.hash(req.password)
        user = User(email=req.email, hashed_password=hashed_password)
        db.add(user)
        db.commit()
        db.refresh(user)
        return {"message": "User registered successfully", "user_id": user.id}
