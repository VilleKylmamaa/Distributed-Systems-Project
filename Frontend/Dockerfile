FROM node:18.14.0

# Change working directory to newly created app dir
WORKDIR /src
EXPOSE 80
EXPOSE 443

COPY ./Frontend/entrypoint.sh /entrypoint.sh
RUN chmod +x /entrypoint.sh

# Copy package.json and package-lock.json to /app dir
COPY ./Frontend/package.json .

# Install dependencies
RUN npm install

# Copy the source code to /app dir
COPY . .

# Expose port 3000 on the container
EXPOSE 3000

ENTRYPOINT ["/entrypoint.sh"]

# Run the app
CMD ["npm", "run", "dev"]
