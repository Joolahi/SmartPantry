from database import SessionLocal
from models import User
from fastapi import HTTPException
from passlib.context import CryptContext
import jwt
import os
from datetime import datetime, timedelta
from dotenv import load_dotenv

load_dotenv()   
pwd_context = CryptContext(schemes=["sha256_crypt"], deprecated="auto") 
SECRET_KEY = os.getenv("SECRET_KEY")
def login_user(req):
    with SessionLocal() as db:
        user = db.query(User).filter(User.email == req.email).first()
        if not user or not pwd_context.verify(req.password, user.hashed_password):
            raise HTTPException(status_code=400, detail="Invalid email or password")
        
        token_data = {
            "sub" : user.id,
            "email": user.email,
            "exp" : datetime.today() + timedelta(days=30)
        }

        token = jwt.encode(token_data, SECRET_KEY, algorithm="HS256")
    
        return {"message": "Login successful", "user_id": user.id, "token": token, "name": user.email}