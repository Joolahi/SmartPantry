from sqlalchemy import Column, Integer, String
from database import Base

class User(Base):
    __tablename__ = "users"

    id = Column(Integer, primary_key=True, index=True)
    email = Column(String, unique=True, index=True)
    hashed_password = Column(String)

class BarcodeEntry(Base):
    __tablename__ = "barcode_entries"

    id = Column(Integer, primary_key=True, index=True)
    barcode = Column(String, index=True)
    name = Column(String)
    brand = Column(String)
    energy = Column(Integer)
    image = Column(String)
    best_before = Column(String, nullable=True)
    user_id = Column(String, nullable=True)
