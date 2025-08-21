from fastapi import FastAPI
import models, database
from routers import pantry

models.Base.metadata.create_all(bind=database.engine)

app = FastAPI()

app.include_router(pantry.router)
