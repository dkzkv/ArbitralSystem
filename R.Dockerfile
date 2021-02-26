
FROM python:3.6.12-alpine
FROM rocker/r-base:4.0.3

#
WORKDIR /app

#
RUN apt update && \
    apt -y install --no-install-recommends \
    python3-pip python-dev

#
COPY requirements.txt ./

RUN pip install --no-cache-dir -r requirements.txt


#
COPY analysis/. .

CMD RScript __init.R